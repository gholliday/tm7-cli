using System.CommandLine;
using Spectre.Console;
using Tm7.Cli.Model;

namespace Tm7.Cli.Commands;

internal static class AddCommand
{
    internal static Command Create()
    {
        var addCmd = new Command("add", "Add elements to the model.");
        addCmd.Add(CreateAddEntityCommand());
        addCmd.Add(CreateAddFlowCommand());
        return addCmd;
    }

    static Command CreateAddEntityCommand()
    {
        var fileArg = new Argument<FileInfo>("file") { Description = "Path to a .tm7 file." };
        var nameOpt = new Option<string>("--name") { Description = "Entity name.", Required = true };
        var typeIdOpt = new Option<string>("--type-id") { Description = "TypeId for the entity.", Required = true };
        var genericTypeIdOpt = new Option<string>("--generic-type-id") { Description = "GenericTypeId (GE.P, GE.DS, GE.EI, GE.TB.B).", Required = true };
        var leftOpt = new Option<int>("--left") { Description = "Left position.", Required = true };
        var topOpt = new Option<int>("--top") { Description = "Top position.", Required = true };
        var surfaceOpt = new Option<int>("--surface") { Description = "Drawing surface index (default 0).", DefaultValueFactory = _ => 0 };
        var widthOpt = new Option<int?>("--width") { Description = "Width (default 100, or 500 for boundaries)." };
        var heightOpt = new Option<int?>("--height") { Description = "Height (default 100, or 400 for boundaries)." };

        var cmd = new Command("entity", "Add a new entity.") { fileArg, nameOpt, typeIdOpt, genericTypeIdOpt, leftOpt, topOpt, surfaceOpt, widthOpt, heightOpt };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var name = parseResult.GetValue(nameOpt)!;
            var typeId = parseResult.GetValue(typeIdOpt)!;
            var genericTypeId = parseResult.GetValue(genericTypeIdOpt)!;
            var left = parseResult.GetValue(leftOpt);
            var top = parseResult.GetValue(topOpt);
            var surfaceIdx = parseResult.GetValue(surfaceOpt);
            var widthVal = parseResult.GetValue(widthOpt);
            var heightVal = parseResult.GetValue(heightOpt);

            bool isBoundary = genericTypeId == "GE.TB.B";
            int width = widthVal ?? (isBoundary ? 500 : 100);
            int height = heightVal ?? (isBoundary ? 400 : 100);

            var model = Tm7File.Load(file.FullName);
            if (model.DrawingSurfaceList is null || surfaceIdx >= model.DrawingSurfaceList.Count)
            {
                AnsiConsole.MarkupLine("[red]Invalid surface index.[/]");
                return;
            }

            var surface = model.DrawingSurfaceList[surfaceIdx];
            var guid = Guid.NewGuid();
            var props = CommandHelpers.CreateEntityProperties(name);
            var stencil = CommandHelpers.CreateStencil(genericTypeId, guid, typeId, props, left, top, width, height);

            if (surface.Borders is null)
            {
                // Borders is private set; use reflection to initialize
                typeof(SerializableDrawingSurfaceModel).GetProperty(nameof(SerializableDrawingSurfaceModel.Borders))!
                    .SetValue(surface, new Dictionary<Guid, object>());
            }
            surface.Borders![guid] = stencil;

            Tm7File.Save(model, file.FullName);
            AnsiConsole.MarkupLine($"[green]Added entity[/] {Markup.Escape(name)} [dim]({guid})[/]");
        });
        return cmd;
    }

    static Command CreateAddFlowCommand()
    {
        var fileArg = new Argument<FileInfo>("file") { Description = "Path to a .tm7 file." };
        var nameOpt = new Option<string>("--name") { Description = "Flow name.", Required = true };
        var sourceOpt = new Option<string>("--source") { Description = "Source entity GUID.", Required = true };
        var targetOpt = new Option<string>("--target") { Description = "Target entity GUID.", Required = true };
        var typeIdOpt = new Option<string>("--type-id") { Description = "TypeId (default: SE.DF.TMCore.Request).", DefaultValueFactory = _ => "SE.DF.TMCore.Request" };
        var surfaceOpt = new Option<int>("--surface") { Description = "Drawing surface index (default 0).", DefaultValueFactory = _ => 0 };

        var cmd = new Command("flow", "Add a new data flow.") { fileArg, nameOpt, sourceOpt, targetOpt, typeIdOpt, surfaceOpt };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var name = parseResult.GetValue(nameOpt)!;
            var sourceGuidStr = parseResult.GetValue(sourceOpt)!;
            var targetGuidStr = parseResult.GetValue(targetOpt)!;
            var typeId = parseResult.GetValue(typeIdOpt)!;
            var surfaceIdx = parseResult.GetValue(surfaceOpt);

            if (!Guid.TryParse(sourceGuidStr, out var sourceGuid) || !Guid.TryParse(targetGuidStr, out var targetGuid))
            {
                AnsiConsole.MarkupLine("[red]Invalid GUID format.[/]");
                return;
            }

            var model = Tm7File.Load(file.FullName);
            if (model.DrawingSurfaceList is null || surfaceIdx >= model.DrawingSurfaceList.Count)
            {
                AnsiConsole.MarkupLine("[red]Invalid surface index.[/]");
                return;
            }

            var surface = model.DrawingSurfaceList[surfaceIdx];

            // Calculate coordinates from source/target entity centers
            int srcX = 0, srcY = 0, tgtX = 0, tgtY = 0;
            if (surface.Borders is not null)
            {
                if (surface.Borders.TryGetValue(sourceGuid, out var srcObj) && srcObj is SerializableBorder srcBorder)
                {
                    srcX = srcBorder.Left + srcBorder.Width / 2;
                    srcY = srcBorder.Top + srcBorder.Height / 2;
                }
                if (surface.Borders.TryGetValue(targetGuid, out var tgtObj) && tgtObj is SerializableBorder tgtBorder)
                {
                    tgtX = tgtBorder.Left + tgtBorder.Width / 2;
                    tgtY = tgtBorder.Top + tgtBorder.Height / 2;
                }
            }

            int handleX = (srcX + tgtX) / 2;
            int handleY = (srcY + tgtY) / 2;

            var guid = Guid.NewGuid();
            var props = CommandHelpers.CreateFlowProperties(name);
            var connector = new SerializableConnector(
                guid, typeId, "GE.DF", props,
                targetGuid, sourceGuid,
                StencilConnectionPort.None, StencilConnectionPort.None,
                srcX, srcY, tgtX, tgtY, handleX, handleY,
                1.0, "");

            if (surface.Lines is null)
            {
                typeof(SerializableDrawingSurfaceModel).GetProperty(nameof(SerializableDrawingSurfaceModel.Lines))!
                    .SetValue(surface, new Dictionary<Guid, object>());
            }
            surface.Lines![guid] = connector;

            Tm7File.Save(model, file.FullName);
            AnsiConsole.MarkupLine($"[green]Added flow[/] {Markup.Escape(name)} [dim]({guid})[/]");
        });
        return cmd;
    }
}
