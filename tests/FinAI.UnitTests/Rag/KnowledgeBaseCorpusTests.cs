using FinAI.AI.Rag.Ingestion;

namespace FinAI.UnitTests.Rag;

public class KnowledgeBaseCorpusTests
{
    private static string KnowledgeBaseFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "FinAI.slnx")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir.FullName, "knowledge-base");
    }

    [Fact]
    public void Every_document_parses_with_unique_doc_id_and_chunks()
    {
        var files = Directory.EnumerateFiles(KnowledgeBaseFolder(), "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.NotEmpty(files);

        var docs = files.Select(f => MarkdownChunker.Parse(File.ReadAllText(f))).ToList();

        Assert.Equal(docs.Count, docs.Select(d => d.DocId).Distinct().Count());
        Assert.All(docs, d => Assert.NotEmpty(d.Chunks));
    }
}
