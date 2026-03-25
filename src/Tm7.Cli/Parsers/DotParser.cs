using System.Text.RegularExpressions;

namespace Tm7.Cli.Parsers;

public record DotEntity(string Id, string Label, bool IsInsideBoundary);
public record DotEdge(string SourceId, string TargetId, string Label, bool Bidirectional);
public record DotBoundary(string Id, string Label, List<string> ContainedEntityIds);
public record DotGraph(string? Label, List<DotEntity> Entities, List<DotEdge> Edges, List<DotBoundary> Boundaries);

public static class DotParser
{
    public static DotGraph Parse(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith("//") && !l.StartsWith("@"))
            .ToList();

        var entities = new Dictionary<string, string>(); // id -> label
        var edges = new List<DotEdge>();
        var boundaries = new List<DotBoundary>();
        var boundaryEntityIds = new HashSet<string>();
        string? graphLabel = null;

        // Join multi-line label statement (the graph label spans multiple lines)
        var joined = JoinMultiLineStatements(lines);

        // Track which boundary context we're in
        var boundaryStack = new Stack<(string id, string label, List<string> entityIds)>();
        bool inClusterBoundary = false;

        foreach (var line in joined)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("//") || trimmed.StartsWith("@")) continue;

            // Skip top-level graph declaration
            if (trimmed.StartsWith("digraph ")) continue;

            // Skip global attributes
            if (trimmed.StartsWith("node [") || trimmed.StartsWith("edge [") ||
                trimmed.StartsWith("fontname=") || trimmed.StartsWith("fontsize=") ||
                trimmed.StartsWith("rankdir=")) continue;
            if (Regex.IsMatch(trimmed, @"^(node|edge)\s+\[")) continue;
            // Skip lines that are just global graph attributes
            if (Regex.IsMatch(trimmed, @"^(fontname|fontsize|rankdir)\s*=")) continue;
            // Combined attribute lines like "fontname=Helvetica fontsize=12 rankdir=LR"
            if (Regex.IsMatch(trimmed, @"^fontname=\S+\s+fontsize=")) continue;

            // Closing brace
            if (trimmed == "}" || trimmed == "};")
            {
                if (boundaryStack.Count > 0)
                {
                    var b = boundaryStack.Pop();
                    if (b.label.Length > 0) // only named clusters become boundaries
                    {
                        boundaries.Add(new DotBoundary(b.id, b.label, b.entityIds));
                        foreach (var eid in b.entityIds)
                            boundaryEntityIds.Add(eid);
                    }
                    else
                    {
                        // Anonymous subgraph - entities belong to parent boundary
                        if (boundaryStack.Count > 0)
                        {
                            foreach (var eid in b.entityIds)
                                boundaryStack.Peek().entityIds.Add(eid);
                        }
                        else
                        {
                            // Mark as inside boundary if this anon subgraph was inside a cluster
                            if (inClusterBoundary)
                            {
                                // They'll be picked up when the parent cluster closes
                            }
                        }
                    }
                    if (boundaryStack.Count == 0)
                        inClusterBoundary = false;
                }
                continue;
            }

            // Subgraph cluster
            var clusterMatch = Regex.Match(trimmed, @"^subgraph\s+cluster_(\w+)\s*\{");
            if (clusterMatch.Success)
            {
                var clusterId = clusterMatch.Groups[1].Value;
                boundaryStack.Push((clusterId, "", new List<string>()));
                inClusterBoundary = true;
                // Parse inline attributes for label
                var rest = trimmed[(clusterMatch.Index + clusterMatch.Length)..];
                // Attributes may follow on same or next lines
                continue;
            }

            // Anonymous subgraph
            if (Regex.IsMatch(trimmed, @"^subgraph\s*\{"))
            {
                boundaryStack.Push(("anon_" + boundaries.Count, "", new List<string>()));
                continue;
            }

            // Cluster attributes (inside subgraph block)
            if (boundaryStack.Count > 0 && IsAttributeLine(trimmed))
            {
                // Check if line contains a label attribute (standalone attribute line, not a node/edge)
                var labelInLine = Regex.Match(trimmed, @"label\s*=\s*""([^""]+)""");
                if (labelInLine.Success)
                {
                    var top = boundaryStack.Pop();
                    boundaryStack.Push((top.id, labelInLine.Groups[1].Value, top.entityIds));
                }
                continue;
            }

            // Graph-level label (multi-line)
            var graphLabelMatch = Regex.Match(trimmed, @"^label\s*=\s*""(.+?)""", RegexOptions.Singleline);
            if (graphLabelMatch.Success && boundaryStack.Count == 0)
            {
                graphLabel = graphLabelMatch.Groups[1].Value
                    .Replace("\\l", "\n").Replace("\\n", "\n").Trim();
                continue;
            }
            // Unquoted graph label that might start with label="
            if (trimmed.StartsWith("label=") && boundaryStack.Count == 0)
            {
                graphLabel = ExtractQuotedString(trimmed, "label=");
                continue;
            }

            // Multi-source edge: { a b } -> target [attrs]
            var multiSrcMatch = Regex.Match(trimmed, @"^\{\s*(.+?)\s*\}\s*->\s*(\w+)\s*(\[.*?\])?\s*;?\s*$");
            if (multiSrcMatch.Success)
            {
                var sources = multiSrcMatch.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var target = multiSrcMatch.Groups[2].Value;
                var attrs = multiSrcMatch.Groups[3].Success ? multiSrcMatch.Groups[3].Value : "";
                var edgeLabel = ExtractAttr(attrs, "label") ?? "";
                var bidir = ExtractAttr(attrs, "dir") == "both";
                foreach (var src in sources)
                {
                    EnsureEntity(entities, src);
                    EnsureEntity(entities, target);
                    edges.Add(new DotEdge(src, target, edgeLabel, bidir));
                    if (boundaryStack.Count > 0)
                    {
                        AddToBoundary(boundaryStack, src);
                        AddToBoundary(boundaryStack, target);
                    }
                }
                continue;
            }

            // Multi-target edge: source -> { a b } [attrs]
            var multiTgtMatch = Regex.Match(trimmed, @"^(\w+)\s*->\s*\{\s*(.+?)\s*\}\s*(\[.*?\])?\s*;?\s*$");
            if (multiTgtMatch.Success)
            {
                var source = multiTgtMatch.Groups[1].Value;
                var targets = multiTgtMatch.Groups[2].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var attrs = multiTgtMatch.Groups[3].Success ? multiTgtMatch.Groups[3].Value : "";
                var edgeLabel = ExtractAttr(attrs, "label") ?? "";
                var bidir = ExtractAttr(attrs, "dir") == "both";
                foreach (var tgt in targets)
                {
                    EnsureEntity(entities, source);
                    EnsureEntity(entities, tgt);
                    edges.Add(new DotEdge(source, tgt, edgeLabel, bidir));
                    if (boundaryStack.Count > 0)
                    {
                        AddToBoundary(boundaryStack, source);
                        AddToBoundary(boundaryStack, tgt);
                    }
                }
                continue;
            }

            // Simple edge: a -> b [attrs]
            var edgeMatch = Regex.Match(trimmed, @"^(\w+)\s*->\s*(\w+)\s*(\[.*?\])?\s*;?\s*$");
            if (edgeMatch.Success)
            {
                var src = edgeMatch.Groups[1].Value;
                var tgt = edgeMatch.Groups[2].Value;
                var attrs = edgeMatch.Groups[3].Success ? edgeMatch.Groups[3].Value : "";
                var edgeLabel = ExtractAttr(attrs, "label") ?? "";
                var bidir = ExtractAttr(attrs, "dir") == "both";
                EnsureEntity(entities, src);
                EnsureEntity(entities, tgt);
                edges.Add(new DotEdge(src, tgt, edgeLabel, bidir));
                if (boundaryStack.Count > 0)
                {
                    AddToBoundary(boundaryStack, src);
                    AddToBoundary(boundaryStack, tgt);
                }
                continue;
            }

            // Node declaration: id [label="...", ...]
            var nodeMatch = Regex.Match(trimmed, @"^(\w+)\s*\[(.+?)\]\s*;?\s*$");
            if (nodeMatch.Success)
            {
                var nodeId = nodeMatch.Groups[1].Value;
                var attrs = nodeMatch.Groups[2].Value;
                var label = ExtractAttr("[" + attrs + "]", "label") ?? nodeId;
                entities[nodeId] = label;
                if (boundaryStack.Count > 0)
                    AddToBoundary(boundaryStack, nodeId);
                continue;
            }
        }

        // Build final entity list
        var entityList = entities.Select(kvp =>
            new DotEntity(kvp.Key, kvp.Value, boundaryEntityIds.Contains(kvp.Key)))
            .ToList();

        return new DotGraph(graphLabel, entityList, edges, boundaries);
    }

    static List<string> JoinMultiLineStatements(List<string> lines)
    {
        var result = new List<string>();
        string? pending = null;
        int quoteCount = 0;

        foreach (var line in lines)
        {
            if (pending != null)
            {
                pending += "\n" + line;
                quoteCount += CountUnescapedQuotes(line);
                if (quoteCount % 2 == 0)
                {
                    result.Add(pending);
                    pending = null;
                    quoteCount = 0;
                }
            }
            else
            {
                quoteCount = CountUnescapedQuotes(line);
                if (quoteCount % 2 != 0)
                {
                    pending = line;
                }
                else
                {
                    result.Add(line);
                }
            }
        }
        if (pending != null)
            result.Add(pending);

        return result;
    }

    static int CountUnescapedQuotes(string s)
    {
        int count = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '"' && (i == 0 || s[i - 1] != '\\'))
                count++;
        }
        return count;
    }

    static void EnsureEntity(Dictionary<string, string> entities, string id)
    {
        if (!entities.ContainsKey(id))
            entities[id] = id; // Use id as default label
    }

    static void AddToBoundary(Stack<(string id, string label, List<string> entityIds)> stack, string entityId)
    {
        var top = stack.Peek();
        if (!top.entityIds.Contains(entityId))
            top.entityIds.Add(entityId);
    }

    static string? ExtractAttr(string attrs, string name)
    {
        // Match: name="value" or name=value
        var match = Regex.Match(attrs, name + @"\s*=\s*""([^""]*?)""");
        if (match.Success) return match.Groups[1].Value;
        match = Regex.Match(attrs, name + @"\s*=\s*(\w+)");
        if (match.Success) return match.Groups[1].Value;
        return null;
    }

    static string? ExtractQuotedString(string line, string prefix)
    {
        var idx = line.IndexOf(prefix);
        if (idx < 0) return null;
        var rest = line[(idx + prefix.Length)..].Trim();
        if (rest.StartsWith("\""))
        {
            // Find closing quote
            var end = rest.LastIndexOf('"');
            if (end > 0)
                return rest[1..end].Replace("\\l", "\n").Replace("\\n", "\n").Trim();
        }
        return rest;
    }

    static bool IsAttributeLine(string trimmed)
    {
        // A line consisting only of key=value attribute pairs (no -> edges, no standalone identifiers with [])
        if (trimmed.Contains("->")) return false;
        // Check if line matches pattern: word=value [word=value ...]
        var stripped = Regex.Replace(trimmed, @"""[^""]*""", ""); // remove quoted strings
        // If what's left is only attribute assignments and whitespace
        return Regex.IsMatch(stripped, @"^(\w+=\S*\s*)+$");
    }
}
