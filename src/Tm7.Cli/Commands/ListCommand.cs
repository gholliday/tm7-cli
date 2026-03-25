using System.CommandLine;
using Spectre.Console;
using Tm7.Cli.Model;

namespace Tm7.Cli.Commands;

internal static class ListCommand
{
    internal static Command Create()
    {
        var listCmd = new Command("list", "List model elements.");
        listCmd.Add(CreateListEntitiesCommand());
        listCmd.Add(CreateListFlowsCommand());
        return listCmd;
    }

    static Command CreateListEntitiesCommand()
    {
        var fileArg = new Argument<FileInfo>("file") { Description = "Path to a .tm7 file." };
        var cmd = new Command("entities", "List all entities.") { fileArg };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var model = Tm7File.Load(file.FullName);

            var table = new Table()
                .AddColumn("GUID")
                .AddColumn("Name")
                .AddColumn("Type")
                .AddColumn("Kind")
                .AddColumn("Surface")
                .AddColumn("Left")
                .AddColumn("Top");

            if (model.DrawingSurfaceList is not null)
            {
                foreach (var surface in model.DrawingSurfaceList)
                {
                    if (surface.Borders is null) continue;
                    foreach (var kvp in surface.Borders)
                    {
                        if (kvp.Value is SerializableBorder border)
                        {
                            table.AddRow(
                                Markup.Escape(border.Guid.ToString()),
                                Markup.Escape(CommandHelpers.GetEntityName(border) ?? "(unnamed)"),
                                Markup.Escape(border.TypeId ?? ""),
                                Markup.Escape(CommandHelpers.MapGenericTypeId(border.GenericTypeId)),
                                Markup.Escape(surface.Header ?? ""),
                                border.Left.ToString(),
                                border.Top.ToString());
                        }
                    }
                }
            }

            AnsiConsole.Write(table);
        });
        return cmd;
    }

    static Command CreateListFlowsCommand()
    {
        var fileArg = new Argument<FileInfo>("file") { Description = "Path to a .tm7 file." };
        var cmd = new Command("flows", "List all data flows.") { fileArg };

        cmd.SetAction(parseResult =>
        {
            var file = parseResult.GetValue(fileArg)!;
            var model = Tm7File.Load(file.FullName);

            var table = new Table()
                .AddColumn("GUID")
                .AddColumn("Name")
                .AddColumn("Source")
                .AddColumn("Source GUID")
                .AddColumn("Target")
                .AddColumn("Target GUID")
                .AddColumn("Surface");

            if (model.DrawingSurfaceList is not null)
            {
                foreach (var surface in model.DrawingSurfaceList)
                {
                    if (surface.Lines is null) continue;
                    foreach (var kvp in surface.Lines)
                    {
                        if (kvp.Value is SerializableConnector conn)
                        {
                            string sourceName = "(unknown)", targetName = "(unknown)";
                            if (surface.Borders is not null)
                            {
                                if (surface.Borders.TryGetValue(conn.SourceGuid, out var srcObj) && srcObj is SerializableTaggable srcTag)
                                    sourceName = CommandHelpers.GetEntityName(srcTag) ?? "(unnamed)";
                                if (surface.Borders.TryGetValue(conn.TargetGuid, out var tgtObj) && tgtObj is SerializableTaggable tgtTag)
                                    targetName = CommandHelpers.GetEntityName(tgtTag) ?? "(unnamed)";
                            }

                            table.AddRow(
                                Markup.Escape(conn.Guid.ToString()),
                                Markup.Escape(CommandHelpers.GetEntityName(conn) ?? "(unnamed)"),
                                Markup.Escape(sourceName),
                                Markup.Escape(conn.SourceGuid.ToString()),
                                Markup.Escape(targetName),
                                Markup.Escape(conn.TargetGuid.ToString()),
                                Markup.Escape(surface.Header ?? ""));
                        }
                    }
                }
            }

            AnsiConsole.Write(table);
        });
        return cmd;
    }
}
