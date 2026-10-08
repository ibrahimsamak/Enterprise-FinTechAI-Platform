using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FinAI.AI.Rag.Ingestion;

public static partial class MarkdownChunker
{
    public const int MaxChunkChars = 2000;
    private const int MaxOverlapChars = 400;

    private static readonly string[] RequiredKeys = ["doc_id", "title", "category", "audience", "version"];

    public static KnowledgeDocument Parse(string markdown)
    {
        var lines = markdown.ReplaceLineEndings("\n").Split('\n');
        var (meta, bodyStart) = ReadFrontMatter(lines);

        var audience = meta["audience"];
        if (audience is not (KnowledgeAudience.Public or KnowledgeAudience.Internal))
            throw new InvalidDataException($"{meta["doc_id"]}: audience must be 'public' or 'internal', was '{audience}'.");

        return new KnowledgeDocument(
            meta["doc_id"], meta["title"], meta["category"], audience, meta["version"],
            Hash(markdown), SplitSections(lines[bodyStart..]));
    }

    private static (Dictionary<string, string> Meta, int BodyStart) ReadFrontMatter(string[] lines)
    {
        if (lines.Length == 0 || lines[0].Trim() != "---")
            throw new InvalidDataException("Document must start with YAML front matter ('---').");

        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "---")
            {
                var missing = RequiredKeys.Where(k => !meta.ContainsKey(k)).ToList();
                if (missing.Count > 0)
                    throw new InvalidDataException($"Front matter is missing: {string.Join(", ", missing)}.");
                return (meta, i + 1);
            }

            var colon = lines[i].IndexOf(':');
            if (colon <= 0) continue;
            meta[lines[i][..colon].Trim()] = lines[i][(colon + 1)..].Trim().Trim('"', '\'');
        }
        throw new InvalidDataException("Front matter is not closed with '---'.");
    }

    private static List<DocumentChunk> SplitSections(string[] body)
    {
        var chunks = new List<DocumentChunk>();
        string? h2 = null, h3 = null;
        var buffer = new StringBuilder();
        var inFence = false;

        void Flush()
        {
            var text = buffer.ToString().Trim();
            buffer.Clear();
            if (text.Length == 0) return;

            var deepest = h3 ?? h2 ?? "Overview";
            var heading = h3 is null ? deepest : $"{h2} > {h3}";
            foreach (var part in SplitLong(text))
                chunks.Add(new DocumentChunk(SectionNumber(deepest), heading, part));
        }

        foreach (var line in body)
        {
            if (line.TrimStart().StartsWith("```")) inFence = !inFence;

            if (!inFence && line.StartsWith("# ")) continue;
            if (!inFence && line.StartsWith("## ")) { Flush(); h2 = line[3..].Trim(); h3 = null; continue; }
            if (!inFence && line.StartsWith("### ")) { Flush(); h3 = line[4..].Trim(); continue; }
            buffer.Append(line).Append('\n');
        }
        Flush();
        return chunks;
    }

    private static IEnumerable<string> SplitLong(string text)
    {
        if (text.Length <= MaxChunkChars)
        {
            yield return text;
            yield break;
        }

        var current = new List<string>();
        var length = 0;
        foreach (var p in ParagraphRegex().Split(text))
        {
            if (length + p.Length > MaxChunkChars && current.Count > 0)
            {
                yield return string.Join("\n\n", current);
                var overlap = current[^1];
                current = overlap.Length <= MaxOverlapChars ? [overlap] : [];
                length = current.Sum(c => c.Length);
            }
            current.Add(p);
            length += p.Length;
        }
        if (current.Count > 0) yield return string.Join("\n\n", current);
    }

    private static string SectionNumber(string heading)
    {
        var m = SectionNumberRegex().Match(heading);
        return m.Success ? m.Groups[1].Value : heading;
    }

    private static string Hash(string content) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

    [GeneratedRegex(@"^(\d+(?:\.\d+)*)\.?\s")]
    private static partial Regex SectionNumberRegex();

    [GeneratedRegex(@"\n\s*\n")]
    private static partial Regex ParagraphRegex();
}
