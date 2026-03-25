using Tm7.Cli.Model;
using Tm7.Cli;
using Xunit;

namespace Tm7.Tests;

public class RoundtripTests
{
    private static string GetSamplePath(string filename)
    {
        // With UseArtifactsOutput, base dir is artifacts/bin/Tm7.Tests/debug/
        // Navigate up 4 levels to the repo root
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "samples", filename);
        return Path.GetFullPath(path);
    }

    private static SerializableModelData LoadTemplate() =>
        Tm7File.Load(GetSamplePath("template.tm7"));

    [Fact]
    public void RoundTrip_ByteForByte()
    {
        var path = GetSamplePath("template.tm7");
        var originalBytes = File.ReadAllBytes(path);

        var model = Tm7File.Load(path);

        using var ms = new MemoryStream();
        Tm7XmlSerializer.Serialize(ms, model);
        var roundTrippedBytes = ms.ToArray();

        Assert.Equal(originalBytes.Length, roundTrippedBytes.Length);
        Assert.True(originalBytes.AsSpan().SequenceEqual(roundTrippedBytes),
            "Round-tripped bytes do not match the original file");
    }

    [Fact]
    public void Load_ModelStructure()
    {
        var model = LoadTemplate();

        Assert.Equal("4.3", model.Version);
        Assert.Equal(2, model.DrawingSurfaceList.Count);

        var first = model.DrawingSurfaceList[0];
        Assert.Equal("Diagram 1", first.Header);
        Assert.Equal(17, first.Borders.Count);
        Assert.Equal(2, first.Lines.Count);

        var second = model.DrawingSurfaceList[1];
        Assert.Equal("Diagram 2", second.Header);
        Assert.Single(second.Borders);
        Assert.Single(second.Lines);
    }

    [Fact]
    public void Load_EntityTypes()
    {
        var model = LoadTemplate();
        var borders = model.DrawingSurfaceList[0].Borders.Values;
        var lines = model.DrawingSurfaceList[0].Lines.Values;

        // StencilEllipse (process) with GenericTypeId "GE.P"
        var ellipses = borders.OfType<SerializableStencilEllipse>().ToList();
        Assert.Contains(ellipses, e => e.GenericTypeId == "GE.P");

        // StencilRectangle (external interactor) with GenericTypeId "GE.EI"
        var rectangles = borders.OfType<SerializableStencilRectangle>().ToList();
        Assert.Contains(rectangles, e => e.GenericTypeId == "GE.EI");

        // StencilParallelLines (data store) with GenericTypeId "GE.DS"
        var parallelLines = borders.OfType<SerializableStencilParallelLines>().ToList();
        Assert.Contains(parallelLines, e => e.GenericTypeId == "GE.DS");

        // BorderBoundary (trust boundary) with GenericTypeId "GE.TB.B"
        var boundaries = borders.OfType<SerializableBorderBoundary>().ToList();
        Assert.Contains(boundaries, e => e.GenericTypeId == "GE.TB.B");

        // Verify specific entity names
        var allEntities = borders.OfType<SerializableTaggable>().ToList();
        var entityNames = allEntities
            .SelectMany(e => e.Properties.OfType<SerializableStringDisplayAttribute>())
            .Where(p => p.DisplayName == "Name")
            .Select(p => p.Value?.ToString()!)
            .ToList();

        Assert.Contains("Browser", entityNames);
        Assert.Contains("Generic Process", entityNames);
        Assert.Contains("Azure AD", entityNames);
        Assert.Contains("Generic Data Store", entityNames);
        Assert.Contains("Azure Cosmos DB", entityNames);
        Assert.Contains("Azure Key Vault", entityNames);
    }

    [Fact]
    public void Load_EntityProperties()
    {
        var model = LoadTemplate();
        var borders = model.DrawingSurfaceList[0].Borders.Values;

        // Find "Browser" entity
        var browser = borders.OfType<SerializableStencilRectangle>()
            .First(e => e.Properties.OfType<SerializableStringDisplayAttribute>()
                .Any(p => p.DisplayName == "Name" && p.Value?.ToString() == "Browser"));

        // It has a "Name" StringDisplayAttribute with value "Browser"
        var nameAttr = browser.Properties.OfType<SerializableStringDisplayAttribute>()
            .First(p => p.DisplayName == "Name");
        Assert.Equal("Browser", nameAttr.Value?.ToString());

        // Position properties are reasonable
        Assert.True(browser.Left >= 0, "Left should be non-negative");
        Assert.True(browser.Top >= 0, "Top should be non-negative");
        Assert.True(browser.Width > 0, "Width should be positive");
        Assert.True(browser.Height > 0, "Height should be positive");
    }

    [Fact]
    public void Load_Connectors()
    {
        var model = LoadTemplate();
        var lines = model.DrawingSurfaceList[0].Lines.Values;

        var connectors = lines.OfType<SerializableConnector>().ToList();
        Assert.NotEmpty(connectors);

        // At least one connector has a non-empty endpoint
        Assert.Contains(connectors, c => c.SourceGuid != Guid.Empty || c.TargetGuid != Guid.Empty);

        // All connectors are data flows
        foreach (var connector in connectors)
        {
            Assert.Equal("GE.DF", connector.GenericTypeId);
        }
    }

    [Fact]
    public void Load_KnowledgeBase()
    {
        var model = LoadTemplate();

        Assert.NotNull(model.KnowledgeBase);
        Assert.NotNull(model.KnowledgeBase.GenericElements);
        Assert.NotNull(model.KnowledgeBase.StandardElements);
        Assert.NotNull(model.KnowledgeBase.ThreatCategories);
        Assert.NotNull(model.KnowledgeBase.ThreatTypes);
        Assert.NotEmpty(model.KnowledgeBase.GenericElements);
        Assert.NotEmpty(model.KnowledgeBase.StandardElements);
        Assert.NotEmpty(model.KnowledgeBase.ThreatCategories);
        Assert.NotEmpty(model.KnowledgeBase.ThreatTypes);
        Assert.Equal("Azure Threat Model Template", model.KnowledgeBase.Manifest.Name);
    }

    [Fact]
    public void Load_MetaInformation()
    {
        var model = LoadTemplate();

        Assert.NotNull(model.MetaInformation);
        Assert.Equal("", model.MetaInformation.ThreatModelName);
        Assert.Equal("", model.MetaInformation.Owner);
        Assert.Equal("", model.MetaInformation.Contributors);
        Assert.Equal("", model.MetaInformation.Reviewer);
        Assert.Equal("", model.MetaInformation.HighLevelSystemDescription);
        Assert.Equal("", model.MetaInformation.Assumptions);
        Assert.Equal("", model.MetaInformation.ExternalDependencies);
    }

    [Fact]
    public void Load_Profile()
    {
        var model = LoadTemplate();
        Assert.NotNull(model.Profile);
    }

    [Fact]
    public void Modify_AndSave()
    {
        var model = LoadTemplate();
        var surface = model.DrawingSurfaceList[0];
        var originalBorderCount = surface.Borders.Count;

        // Create a new StencilEllipse (process)
        var newGuid = Guid.NewGuid();
        var newEllipse = new SerializableStencilEllipse(
            guid: newGuid,
            typeId: "StencilEllipse",
            genericTypeId: "GE.P",
            properties: new List<SerializableDisplayAttribute>
            {
                new SerializableStringDisplayAttribute("Name", "Name", "Test Process")
            },
            x: 100, y: 100, width: 160, height: 80,
            strokeThickness: 1.0, strokeDashArray: "");

        surface.Borders.Add(newGuid, newEllipse);

        // Save to a temp file and reload
        var tempPath = Path.Combine(Path.GetDirectoryName(GetSamplePath("template.tm7"))!, "test_output.tm7");
        try
        {
            Tm7File.Save(model, tempPath);

            var reloaded = Tm7File.Load(tempPath);
            var reloadedSurface = reloaded.DrawingSurfaceList[0];

            Assert.Equal(originalBorderCount + 1, reloadedSurface.Borders.Count);
            Assert.True(reloadedSurface.Borders.ContainsKey(newGuid));

            var reloadedEllipse = reloadedSurface.Borders[newGuid] as SerializableStencilEllipse;
            Assert.NotNull(reloadedEllipse);

            var nameAttr = reloadedEllipse!.Properties.OfType<SerializableStringDisplayAttribute>()
                .First(p => p.DisplayName == "Name");
            Assert.Equal("Test Process", nameAttr.Value?.ToString());
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void KnownTypes_Complete()
    {
        // Verify the AOT-safe serializer can be constructed without errors
        using var ms = new MemoryStream();
        var model = LoadTemplate();
        Tm7XmlSerializer.Serialize(ms, model);
        Assert.True(ms.Length > 0);
    }
}
