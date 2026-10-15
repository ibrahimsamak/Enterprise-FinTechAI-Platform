using FinAI.AI.Rag.Ingestion;

namespace FinAI.UnitTests.Rag;

public class MarkdownChunkerTests
{
    private const string FrontMatter = """
        ---
        doc_id: TST-001
        title: Test Policy
        category: fraud
        audience: internal
        version: "1.0"
        ---
        """;

    private static KnowledgeDocument Parse(string body) => MarkdownChunker.Parse(FrontMatter + "\n" + body);

    [Fact]
    public void Parses_front_matter()
    {
        var doc = Parse("# Test Policy\n\n## 1. Purpose\n\nText.");

        Assert.Equal("TST-001", doc.DocId);
        Assert.Equal("Test Policy", doc.Title);
        Assert.Equal("internal", doc.Audience);
        Assert.Equal("1.0", doc.Version);
        Assert.Equal(64, doc.ContentHash.Length);
    }

    [Fact]
    public void Missing_required_key_throws()
    {
        var md = "---\ndoc_id: X\ntitle: T\ncategory: c\nversion: 1\n---\n## 1. A\n\nText.";
        var ex = Assert.Throws<InvalidDataException>(() => MarkdownChunker.Parse(md));
        Assert.Contains("audience", ex.Message);
    }

    [Fact]
    public void Unknown_audience_throws()
    {
        var md = FrontMatter.Replace("audience: internal", "audience: everyone") + "\n## 1. A\n\nText.";
        Assert.Throws<InvalidDataException>(() => MarkdownChunker.Parse(md));
    }

    [Fact]
    public void H3_chunks_carry_heading_path_and_section_number()
    {
        var doc = Parse("## 4. Policy statements\n\n### 4.1 Automated screening\n\nScreen every payment.\n\n## 5. Decisions\n\nApprove or escalate.");

        Assert.Equal(2, doc.Chunks.Count);
        Assert.Equal("4.1", doc.Chunks[0].Section);
        Assert.Equal("4. Policy statements > 4.1 Automated screening", doc.Chunks[0].Heading);
        Assert.Equal("5", doc.Chunks[1].Section);
        Assert.Equal("Approve or escalate.", doc.Chunks[1].Text);
    }

    [Fact]
    public void Headings_inside_code_fences_do_not_split()
    {
        var doc = Parse("## 1. Format\n\n```yaml\n## not a heading\n```\n\nAfter.");

        var chunk = Assert.Single(doc.Chunks);
        Assert.Contains("## not a heading", chunk.Text);
    }

    [Fact]
    public void Long_section_splits_with_overlap()
    {
        var paragraphs = Enumerable.Range(1, 12).Select(i => $"P{i} " + new string('x', 300));
        var doc = Parse("## 1. Long\n\n" + string.Join("\n\n", paragraphs));

        Assert.True(doc.Chunks.Count > 1);
        Assert.All(doc.Chunks, c => Assert.True(c.Text.Length <= MarkdownChunker.MaxChunkChars + 400));
        var lastOfFirst = doc.Chunks[0].Text.Split("\n\n")[^1];
        Assert.StartsWith(lastOfFirst, doc.Chunks[1].Text);
    }
}
