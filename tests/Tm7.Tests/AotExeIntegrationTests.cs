using System.Diagnostics;
using System.Runtime.InteropServices;
using Tm7.Cli.Model;
using Tm7.Cli;
using Xunit;

namespace Tm7.Tests;

/// <summary>
/// Integration tests that invoke the published NativeAOT executable to confirm that the
/// AOT round-trip works for cases the in-process JIT tests cannot reach (the JIT path
/// uses code-gen, the AOT path uses the reflection-based writer/reader). These tests
/// are skipped when the AOT publish output is not present.
/// </summary>
public class AotExeIntegrationTests
{
    private static string RepoRoot()
    {
        // bin/Tm7.Tests/debug/ -> up 4 = repo root
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    }

    private static string SamplePath(string name) => Path.Combine(RepoRoot(), "samples", name);

    private static string? AotExePath()
    {
        var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "tm7.exe" : "tm7";
        // Check both publish layouts:
        //   release/                 -> `dotnet publish -c Release` (no explicit RID; the
        //                               CI workflow uses this layout).
        //   release_<RID>/           -> `dotnet publish -c Release -r <RID>`.
        var candidates = new[]
        {
            Path.Combine(RepoRoot(), "artifacts", "publish", "Tm7.Cli", "release", exeName),
            Path.Combine(RepoRoot(), "artifacts", "publish", "Tm7.Cli", $"release_{RuntimeInformation.RuntimeIdentifier}", exeName),
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private static (int ExitCode, string Stdout, string Stderr) Run(string exe, params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = RepoRoot(),
        };
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi)!;
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit(60_000);
        return (p.ExitCode, stdout, stderr);
    }

    private static SerializableThreat MakeThreat(int id)
    {
        return new SerializableThreat(
            id: id,
            typeId: "ThreatType.GenericInformation",
            sourceGuid: Guid.NewGuid(),
            targetGuid: Guid.NewGuid(),
            flowGuid: Guid.NewGuid(),
            drawingSurfaceGuid: Guid.NewGuid(),
            state: ThreatState.NotApplicable,
            interactionKey: "k-" + id,
            priority: "High",
            wide: false,
            changedBy: "tester",
            modifiedAt: DateTime.UtcNow,
            upgraded: false,
            properties: new Dictionary<string, string>
            {
                ["Title"] = "T" + id,
                ["Description"] = "D" + id,
            });
    }

    [Fact]
    public void AotExe_Open_LoadsTemplate()
    {
        var exe = AotExePath();
        Assert.SkipUnless(exe is not null, "AOT publish output not present; run `dotnet publish src/Tm7.Cli -c Release -r win-x64` first.");

        var (code, stdout, stderr) = Run(exe!, "open", SamplePath("template.tm7"));
        Assert.True(code == 0, $"open exit={code} stderr={stderr}");
        Assert.Contains("Diagram 1", stdout);
    }

    [Fact]
    public void AotExe_AddEntity_RoundTripsAndPreservesExistingEntities()
    {
        var exe = AotExePath();
        Assert.SkipUnless(exe is not null, "AOT publish output not present; run `dotnet publish src/Tm7.Cli -c Release -r win-x64` first.");

        var work = Path.Combine(Path.GetTempPath(), $"tm7-aot-{Guid.NewGuid():N}.tm7");
        File.Copy(SamplePath("template.tm7"), work, overwrite: true);
        try
        {
            var (code, _, stderr) = Run(exe!,
                "add", "entity", work,
                "--name", "AotAdded",
                "--type-id", "StencilEllipse",
                "--generic-type-id", "GE.P",
                "--left", "10", "--top", "10");
            Assert.True(code == 0, $"add exit={code} stderr={stderr}");

            // Reload via JIT and assert structural preservation: original 17 borders + 1 new.
            var reloaded = Tm7File.Load(work);
            Assert.Equal(18, reloaded.DrawingSurfaceList[0].Borders.Count);
        }
        finally
        {
            File.Delete(work);
        }
    }

    [Fact]
    public void AotExe_PopulatedThreatsRoundTripThroughAddCommand()
    {
        // Most important AOT regression guard: JIT-produce a model with populated
        // AllThreatsDictionary + SerializableThreat.Properties, run the AOT exe to
        // load+modify+save it, then JIT-reload and verify the threats and properties
        // were preserved through the AOT write path (which uses the reflection writer
        // we had to refactor for AOT compatibility).
        var exe = AotExePath();
        Assert.SkipUnless(exe is not null, "AOT publish output not present; run `dotnet publish src/Tm7.Cli -c Release -r win-x64` first.");

        var work = Path.Combine(Path.GetTempPath(), $"tm7-aot-threats-{Guid.NewGuid():N}.tm7");
        var model = Tm7File.Load(SamplePath("template.tm7"));
        model.AllThreatsDictionary.Add("t1", MakeThreat(1));
        model.AllThreatsDictionary.Add("t2", MakeThreat(2));
        Tm7File.Save(model, work);

        try
        {
            // Sanity: JIT-written file is readable by JIT.
            var preCheck = Tm7File.Load(work);
            Assert.Equal(2, preCheck.AllThreatsDictionary.Count);

            // Run the AOT exe to load + modify + save the file.
            var (code, _, stderr) = Run(exe!,
                "add", "entity", work,
                "--name", "AotProbe",
                "--type-id", "StencilEllipse",
                "--generic-type-id", "GE.P",
                "--left", "5", "--top", "5");
            Assert.True(code == 0, $"AOT add failed exit={code} stderr={stderr}");

            // Reload via JIT and confirm AOT preserved the threats and their properties.
            var after = Tm7File.Load(work);
            Assert.Equal(2, after.AllThreatsDictionary.Count);
            Assert.True(after.AllThreatsDictionary.ContainsKey("t1"));
            Assert.True(after.AllThreatsDictionary.ContainsKey("t2"));
            var t1 = after.AllThreatsDictionary["t1"];
            Assert.Equal(1, t1.Id);
            Assert.Equal("T1", t1.Properties["Title"]);
            Assert.Equal("D1", t1.Properties["Description"]);
        }
        finally
        {
            File.Delete(work);
        }
    }
}
