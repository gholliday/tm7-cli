using System.CommandLine;
using Spectre.Console;
using Tm7.Cli.Model;

namespace Tm7.Cli.Commands;

internal static class NewCommand
{
    internal static Command Create()
    {
        var fileArg = new Argument<FileInfo>("file") { Description = "Path to the new .tm7 file." };
        var templateOpt = new Option<FileInfo>("--template") { Description = "Path to a template .tm7 file.", Required = true };
        var nameOpt = new Option<string>("--name") { Description = "Model name.", DefaultValueFactory = _ => "New Threat Model" };

        var cmd = new Command("new", "Create a new model from a template.") { fileArg, templateOpt, nameOpt };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var templateFile = parseResult.GetValue(templateOpt)!;
            var modelName = parseResult.GetValue(nameOpt)!;

            var template = Tm7File.Load(templateFile.FullName);

            var emptySurface = new SerializableDrawingSurfaceModel(
                Guid.NewGuid(), "", "", Array.Empty<SerializableDisplayAttribute>(),
                Array.Empty<SerializableBorder>(), Array.Empty<SerializableLine>(),
                100.0, "Diagram 1");

            var meta = new SerializableMetaInformation(modelName, "", "", "", "", "", "");

            var newModel = new SerializableModelData(
                new[] { emptySurface },
                meta,
                Array.Empty<SerializableNote>(),
                new Dictionary<string, SerializableThreat>(),
                true,
                Array.Empty<SerializableValidation>(),
                template.Version,
                template.KnowledgeBase,
                template.Profile ?? new SerializableProfile());

            Tm7File.Save(newModel, file.FullName);
            AnsiConsole.MarkupLine($"[green]Created[/] {Markup.Escape(file.FullName)} [dim](from {Markup.Escape(templateFile.Name)})[/]");
        });
        return cmd;
    }
}
