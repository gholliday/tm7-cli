using System.CommandLine;
using Spectre.Console;
using Tm7.Cli.Model;
using Tm7.Cli.Parsers;

namespace Tm7.Cli.Commands;

internal static class ImportCommand
{
    internal static Command Create()
    {
        var importCmd = new Command("import", "Import from external formats.");
        importCmd.Add(CreateImportDotCommand());
        return importCmd;
    }

    static Command CreateImportDotCommand()
    {
        var dotFileArg = new Argument<FileInfo>("dotfile") { Description = "Path to the .dot file." };
        var outputOpt = new Option<FileInfo>("--output") { Description = "Output .tm7 file path.", Required = true };
        var templateOpt = new Option<FileInfo>("--template") { Description = "Template .tm7 file for KB.", Required = true };

        var cmd = new Command("dot", "Import a Graphviz DOT file into a TM7 model.") { dotFileArg, outputOpt, templateOpt };

        cmd.SetAction(parseResult =>
        {
            var dotFile = parseResult.GetValue(dotFileArg)!;
            var outputFile = parseResult.GetValue(outputOpt)!;
            var templateFile = parseResult.GetValue(templateOpt)!;

            // 1. Parse DOT
            var dotGraph = DotParser.Parse(dotFile.FullName);
            AnsiConsole.MarkupLine($"[blue]Parsed DOT:[/] {dotGraph.Entities.Count} entities, {dotGraph.Edges.Count} edges, {dotGraph.Boundaries.Count} boundaries");

            // 2. Load template for KB
            var template = Tm7File.Load(templateFile.FullName);

            // 3. Build entity GUID map
            var entityGuids = new Dictionary<string, Guid>();
            foreach (var entity in dotGraph.Entities)
                entityGuids[entity.Id] = Guid.NewGuid();

            // 4. Create all border stencils
            var borders = new List<SerializableBorder>();

            // Layout: wide horizontal (LR) layout matching the DOT rankdir=LR
            var externalEntities = dotGraph.Entities.Where(e => !e.IsInsideBoundary).ToList();
            var internalEntities = dotGraph.Entities.Where(e => e.IsInsideBoundary).ToList();

            const int entityW = 150;
            const int entityH = 80;
            const int vGap = 140;  // vertical gap between entities in same column

            // Classify internal entities into columns
            var col0 = new List<DotEntity>(); // left inside boundary: AFD, Entra ID, CORP Graph
            var col1 = new List<DotEntity>(); // center: core processes (cronjobs, Web App)
            var col2a = new List<DotEntity>(); // right-upper: data stores (blobs, cosmos)
            var col2b = new List<DotEntity>(); // right-lower: other (ADX, KV, Postgres, AppInsights)

            foreach (var ent in internalEntities)
            {
                var mapping = DotToTm7Mapper.MapEntityType(ent.Id, ent.Label, true);
                var layoutCol = DotToTm7Mapper.GetLayoutColumn(ent.Id, ent.Label, mapping.GenericTypeId);
                switch (layoutCol)
                {
                    case 0: col0.Add(ent); break;
                    case 1: col1.Add(ent); break;
                    default:
                        // Split col2 into two sub-columns: storage-type vs process-type
                        if (mapping.GenericTypeId == "GE.DS")
                            col2a.Add(ent);
                        else
                            col2b.Add(ent);
                        break;
                }
            }

            // Column X positions — wide spread
            const int extCol = 30;
            const int col0X = 350;
            const int col1X = 700;
            const int col2aX = 1050;   // data stores
            const int col2bX = 1350;   // supporting processes (ADX, AppInsights)

            // Compute max rows to size the canvas
            int maxRows = Math.Max(Math.Max(externalEntities.Count, col0.Count),
                          Math.Max(col1.Count, Math.Max(col2a.Count, col2b.Count)));
            int canvasHeight = Math.Max(600, 60 + maxRows * vGap + 60);

            void PlaceColumn(List<DotEntity> column, int x, bool insideBoundary)
            {
                // Center column vertically within canvas
                int totalHeight = column.Count * entityH + (column.Count - 1) * (vGap - entityH);
                int startY = Math.Max(60, canvasHeight / 2 - totalHeight / 2);
                int y = startY;
                foreach (var ent in column)
                {
                    var mapping = insideBoundary
                        ? DotToTm7Mapper.MapEntityType(ent.Id, ent.Label, true)
                        : DotToTm7Mapper.MapEntityType(ent.Id, ent.Label, false);
                    var guid = entityGuids[ent.Id];
                    var props = CommandHelpers.CreateEntityProperties(ent.Label);
                    borders.Add(CommandHelpers.CreateStencil(mapping.GenericTypeId, guid, mapping.TypeId, props, x, y, entityW, entityH));
                    y += vGap;
                }
            }

            // Place all columns
            PlaceColumn(externalEntities, extCol, false);
            PlaceColumn(col0, col0X, true);
            PlaceColumn(col1, col1X, true);
            PlaceColumn(col2a, col2aX, true);
            PlaceColumn(col2b, col2bX, true);

            // Create boundary: encompass all internal entities with padding
            foreach (var boundary in dotGraph.Boundaries)
            {
                var bMapping = DotToTm7Mapper.MapBoundaryType(boundary.Label);
                var bGuid = Guid.NewGuid();
                var bProps = CommandHelpers.CreateEntityProperties(boundary.Label);
                // Boundary spans from col0 to rightmost column
                int rightEdge = col2b.Count > 0 ? col2bX : col2aX;
                int boundaryLeft = col0X - 40;
                int boundaryWidth = rightEdge + entityW + 40 - boundaryLeft;
                int boundaryTop = 20;
                int boundaryHeight = canvasHeight - 20;
                borders.Add(CommandHelpers.CreateStencil(bMapping.GenericTypeId, bGuid, bMapping.TypeId, bProps, boundaryLeft, boundaryTop, boundaryWidth, boundaryHeight));
            }

            // 5. Create flows with smart port routing
            var flowLines = new List<SerializableLine>();
            foreach (var edge in dotGraph.Edges)
            {
                if (!entityGuids.TryGetValue(edge.SourceId, out var srcGuid) ||
                    !entityGuids.TryGetValue(edge.TargetId, out var tgtGuid))
                    continue;

                var srcBorder = borders.FirstOrDefault(b => b.Guid == srcGuid);
                var tgtBorder = borders.FirstOrDefault(b => b.Guid == tgtGuid);
                if (srcBorder == null || tgtBorder == null) continue;

                // Compute edge attachment points based on relative direction
                // Source exits from the side closest to target; target receives on closest side
                var (srcX, srcY, srcPort) = ComputeEdgePoint(srcBorder, tgtBorder, isSource: true);
                var (tgtX, tgtY, tgtPort) = ComputeEdgePoint(tgtBorder, srcBorder, isSource: false);
                int handleX = (srcX + tgtX) / 2;
                int handleY = (srcY + tgtY) / 2;

                string flowLabel = edge.Label.Length > 0 ? edge.Label : $"{GetEntityLabelShort(dotGraph, edge.SourceId)} -> {GetEntityLabelShort(dotGraph, edge.TargetId)}";

                // Forward flow
                var fwdGuid = Guid.NewGuid();
                var fwdProps = CommandHelpers.CreateFlowProperties(flowLabel);
                flowLines.Add(new SerializableConnector(
                    fwdGuid, "SE.DF.TMCore.Request", "GE.DF", fwdProps,
                    tgtGuid, srcGuid,
                    tgtPort, srcPort,
                    srcX, srcY, tgtX, tgtY, handleX, handleY,
                    1.0, ""));

                // Reverse flow for bidirectional
                if (edge.Bidirectional)
                {
                    string revLabel = edge.Label.Length > 0
                        ? $"{edge.Label} (response)"
                        : $"{GetEntityLabelShort(dotGraph, edge.TargetId)} -> {GetEntityLabelShort(dotGraph, edge.SourceId)}";
                    var revGuid = Guid.NewGuid();
                    var revProps = CommandHelpers.CreateFlowProperties(revLabel);
                    // Reverse: swap src/tgt, use opposite ports
                    var (revSrcX, revSrcY, revSrcPort) = ComputeEdgePoint(tgtBorder, srcBorder, isSource: true);
                    var (revTgtX, revTgtY, revTgtPort) = ComputeEdgePoint(srcBorder, tgtBorder, isSource: false);
                    int revHandleX = (revSrcX + revTgtX) / 2;
                    int revHandleY = (revSrcY + revTgtY) / 2 + 15; // slight offset so labels don't overlap
                    flowLines.Add(new SerializableConnector(
                        revGuid, "SE.DF.TMCore.Response", "GE.DF", revProps,
                        srcGuid, tgtGuid,
                        revTgtPort, revSrcPort,
                        revSrcX, revSrcY, revTgtX, revTgtY, revHandleX, revHandleY,
                        1.0, ""));
                }
            }

            // 6. Build the model
            string modelName = "OSMP Threat Model";
            if (dotGraph.Label != null)
            {
                // Use first sentence of graph label
                var firstLine = dotGraph.Label.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (firstLine != null && firstLine.Length > 10)
                    modelName = firstLine.Length > 100 ? firstLine[..100] : firstLine;
            }

            var surface = new SerializableDrawingSurfaceModel(
                Guid.NewGuid(), "", "", Array.Empty<SerializableDisplayAttribute>(),
                borders.ToArray(), flowLines.ToArray(),
                80.0, "Diagram 1");

            var description = dotGraph.Label ?? "";

            var meta = new SerializableMetaInformation(modelName, "", "", "", description, "", "");

            var newModel = new SerializableModelData(
                new[] { surface },
                meta,
                Array.Empty<SerializableNote>(),
                new Dictionary<string, SerializableThreat>(),
                true,
                Array.Empty<SerializableValidation>(),
                template.Version,
                template.KnowledgeBase,
                template.Profile ?? new SerializableProfile());

            Tm7File.Save(newModel, outputFile.FullName);

            AnsiConsole.MarkupLine($"[green]Imported[/] {Markup.Escape(outputFile.FullName)}");
            AnsiConsole.MarkupLine($"  Entities: {borders.Count} ({externalEntities.Count} external, {internalEntities.Count} internal, {dotGraph.Boundaries.Count} boundaries)");
            AnsiConsole.MarkupLine($"  Flows: {flowLines.Count}");
        });
        return cmd;
    }

    static string GetEntityLabelShort(DotGraph graph, string entityId)
    {
        var entity = graph.Entities.FirstOrDefault(e => e.Id == entityId);
        if (entity == null) return entityId;
        var label = entity.Label.Replace("\\n", " ");
        // Trim multi-line labels to first line
        var newline = label.IndexOf('\n');
        if (newline > 0) label = label[..newline];
        return label.Length > 30 ? label[..30] : label;
    }

    /// <summary>
    /// Computes the edge attachment point on an entity border based on the direction to another entity.
    /// Returns (x, y, port) where the connector should attach.
    /// </summary>
    static (int x, int y, StencilConnectionPort port) ComputeEdgePoint(
        SerializableBorder entity, SerializableBorder other, bool isSource)
    {
        int cx = entity.Left + entity.Width / 2;
        int cy = entity.Top + entity.Height / 2;
        int ox = other.Left + other.Width / 2;
        int oy = other.Top + other.Height / 2;

        int dx = ox - cx;
        int dy = oy - cy;

        // Determine dominant direction
        if (Math.Abs(dx) > Math.Abs(dy))
        {
            // Horizontal: use East or West port
            if (dx > 0)
                return (entity.Left + entity.Width, cy, isSource ? StencilConnectionPort.East : StencilConnectionPort.East);
            else
                return (entity.Left, cy, isSource ? StencilConnectionPort.West : StencilConnectionPort.West);
        }
        else
        {
            // Vertical: use North or South port
            if (dy > 0)
                return (cx, entity.Top + entity.Height, isSource ? StencilConnectionPort.South : StencilConnectionPort.South);
            else
                return (cx, entity.Top, isSource ? StencilConnectionPort.North : StencilConnectionPort.North);
        }
    }
}
