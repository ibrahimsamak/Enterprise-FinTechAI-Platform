using Pgvector;

namespace FinAI.AI.Rag;

public sealed class PolicyChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DocId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Version { get; set; } = default!;
    public string Section { get; set; } = default!;      
    public string Heading { get; set; } = default!;
    public int ChunkIndex { get; set; }
    public string Text { get; set; } = default!;
    public string ContentHash { get; set; } = default!;
    public Vector Embedding { get; set; } = default!;
    public DateTime IndexedOnUtc { get; set; }
}

public static class KnowledgeAudience
{
    public const string Public = "public";
    public const string Internal = "internal";
}
