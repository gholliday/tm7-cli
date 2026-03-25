using System.CommandLine;
using Spectre.Console;

namespace Tm7.Cli.Commands;

internal static class OpenCommand
{
    internal static Command Create()
    {
        var fileArg = new Argument<FileInfo>("file") { Description = "Path to a .tm7 file." };
        var cmd = new Command("open", "Show model summary.") { fileArg };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var model = Tm7File.Load(file.FullName);

            AnsiConsole.MarkupLine($"[bold]Model:[/] {Markup.Escape(model.MetaInformation?.ThreatModelName ?? "(unnamed)")}");
            AnsiConsole.MarkupLine($"[bold]Version:[/] {model.Version}");
            AnsiConsole.MarkupLine($"[bold]Drawing surfaces:[/] {model.DrawingSurfaceList?.Count ?? 0}");

            if (model.DrawingSurfaceList is not null)
            {
                var table = new Table().AddColumn("Surface").AddColumn("Entities").AddColumn("Flows");
                foreach (var s in model.DrawingSurfaceList)
                    table.AddRow(Markup.Escape(s.Header ?? ""), (s.Borders?.Count ?? 0).ToString(), (s.Lines?.Count ?? 0).ToString());
                AnsiConsole.Write(table);
            }

            AnsiConsole.MarkupLine($"[bold]Total threats:[/] {model.AllThreatsDictionary?.Count ?? 0}");
        });
        return cmd;
    }
}
