namespace FinAI.AI.Rag.Ingestion;

public sealed record KnowledgeDocument(
    string DocId, string Title, string Category, string Audience, string Version,
    string ContentHash, IReadOnlyList<DocumentChunk> Chunks);

public sealed record DocumentChunk(string Section, string Heading, string Text);
