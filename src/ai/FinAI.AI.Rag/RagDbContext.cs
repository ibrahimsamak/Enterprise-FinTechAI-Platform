using Microsoft.EntityFrameworkCore;

namespace FinAI.AI.Rag;

public sealed class RagDbContext(DbContextOptions<RagDbContext> o) : DbContext(o)
{
    public DbSet<PolicyChunk> PolicyChunks => Set<PolicyChunk>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("vector");
        b.HasDefaultSchema("rag");
        b.Entity<PolicyChunk>(e =>
        {
            e.ToTable("policy_chunks");
            e.Property(c => c.Embedding).HasColumnType("vector(1536)");
            e.HasIndex(c => c.Embedding).HasMethod("hnsw").HasOperators("vector_cosine_ops");
            e.HasIndex(c => new { c.DocId, c.ChunkIndex }).IsUnique();
            e.HasIndex(c => c.Audience);
        });
    }
}
