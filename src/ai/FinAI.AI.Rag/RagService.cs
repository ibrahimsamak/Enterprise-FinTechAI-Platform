using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace FinAI.AI.Rag;

public sealed class RagService(RagDbContext db, IEmbeddingGenerator<string, Embedding<float>> embedder)
{
    private static readonly ActivitySource Activity = new("FinAI.Rag");
    public async Task<IReadOnlyList<RetrievedChunk>> SearchAsync(
        string query, KnowledgeScope scope, int k = 4, CancellationToken ct = default)
    {
        using var span = Activity.StartActivity("rag search");
        var q = new Vector(await embedder.GenerateVectorAsync(query, cancellationToken: ct));
        var chunks = db.PolicyChunks.AsNoTracking();
        if (scope == KnowledgeScope.Public)
            chunks = chunks.Where(c => c.Audience == KnowledgeAudience.Public);

        return await chunks
            .OrderBy(c => c.Embedding.CosineDistance(q))
            .Take(k)
            .Select(c => new RetrievedChunk(c.DocId, c.Section, c.Title, c.Heading, c.Text, c.Embedding.CosineDistance(q)))
            .ToListAsync(ct);
    }
}
