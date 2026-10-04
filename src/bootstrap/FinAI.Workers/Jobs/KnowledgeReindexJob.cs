using FinAI.AI.Rag.Ingestion;
using Hangfire;

namespace FinAI.Workers.Jobs;

public sealed class KnowledgeReindexJob(RagIngestionService ingestion, ILogger<KnowledgeReindexJob> logger)
{
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public async Task RunAsync(CancellationToken ct)
    {
        var result = await ingestion.IngestDirectoryAsync(KnowledgeBasePath.Find(), force: false, ct);
        logger.LogInformation(
            "Knowledge re-index: {Indexed} indexed, {Skipped} unchanged, {Removed} removed, {Chunks} chunks",
            result.Indexed, result.Skipped, result.Removed, result.Chunks);
    }
}
