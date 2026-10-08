using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace FinAI.AI.Rag.Ingestion;

public sealed class RagIngestionService(
    RagDbContext db, IEmbeddingGenerator<string, Embedding<float>> embedder, ILogger<RagIngestionService> log)
{
    private static readonly ActivitySource Activity = new("FinAI.Rag");
    private const int EmbeddingDimensions = 1536;

    public async Task<IngestionResult> IngestDirectoryAsync(string root, bool force, CancellationToken ct)
    {
        using var span = Activity.StartActivity("rag ingest");
        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"Knowledge base folder not found: {Path.GetFullPath(root)}");

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int indexed = 0, skipped = 0, totalChunks = 0;

        var files = Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal);

        foreach (var file in files)
        {
            var doc = MarkdownChunker.Parse(await File.ReadAllTextAsync(file, ct));
            if (!seen.Add(doc.DocId))
                throw new InvalidDataException($"Duplicate doc_id '{doc.DocId}' in {file}.");
            if (doc.Chunks.Count == 0)
            {
                log.LogWarning("{DocId} ({File}) produced no chunks; skipped.", doc.DocId, file);
                continue;
            }

            var storedHash = await db.PolicyChunks.Where(c => c.DocId == doc.DocId)
                .Select(c => c.ContentHash).FirstOrDefaultAsync(ct);
            if (!force && storedHash == doc.ContentHash)
            {
                skipped++;
                continue;
            }

            var inputs = doc.Chunks.Select(c => $"{doc.Title} - {c.Heading}\n\n{c.Text}").ToList();
            var embeddings = await embedder.GenerateAsync(inputs, cancellationToken: ct);
            if (embeddings.Any(e => e.Vector.Length != EmbeddingDimensions))
                throw new InvalidOperationException(
                    $"Embedding model returned {embeddings[0].Vector.Length} dims; rag.policy_chunks expects {EmbeddingDimensions}.");

            var now = DateTime.UtcNow;
            await using (var tx = await db.Database.BeginTransactionAsync(ct))
            {
                await db.PolicyChunks.Where(c => c.DocId == doc.DocId).ExecuteDeleteAsync(ct);
                db.PolicyChunks.AddRange(doc.Chunks.Select((c, i) => new PolicyChunk
                {
                    DocId = doc.DocId,
                    Title = doc.Title,
                    Category = doc.Category,
                    Audience = doc.Audience,
                    Version = doc.Version,
                    Section = c.Section,
                    Heading = c.Heading,
                    ChunkIndex = i,
                    Text = c.Text,
                    ContentHash = doc.ContentHash,
                    Embedding = new Vector(embeddings[i].Vector),
                    IndexedOnUtc = now
                }));
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            db.ChangeTracker.Clear();

            indexed++;
            totalChunks += doc.Chunks.Count;
            log.LogInformation("Indexed {DocId} v{Version}: {Chunks} chunks", doc.DocId, doc.Version, doc.Chunks.Count);
        }

        var removedIds = await db.PolicyChunks.Where(c => !seen.Contains(c.DocId))
            .Select(c => c.DocId).Distinct().ToListAsync(ct);
        if (removedIds.Count > 0)
            await db.PolicyChunks.Where(c => removedIds.Contains(c.DocId)).ExecuteDeleteAsync(ct);

        return new IngestionResult(indexed, skipped, removedIds.Count, totalChunks);
    }
}

public sealed record IngestionResult(int Indexed, int Skipped, int Removed, int Chunks);
