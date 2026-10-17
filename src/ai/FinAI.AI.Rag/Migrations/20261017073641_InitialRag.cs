using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace FinAI.AI.Rag.Migrations
{
    public partial class InitialRag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rag");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "policy_chunks",
                schema: "rag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Audience = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Section = table.Column<string>(type: "text", nullable: false),
                    Heading = table.Column<string>(type: "text", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    ContentHash = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    IndexedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_chunks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_policy_chunks_Audience",
                schema: "rag",
                table: "policy_chunks",
                column: "Audience");

            migrationBuilder.CreateIndex(
                name: "IX_policy_chunks_DocId_ChunkIndex",
                schema: "rag",
                table: "policy_chunks",
                columns: new[] { "DocId", "ChunkIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_chunks_Embedding",
                schema: "rag",
                table: "policy_chunks",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_chunks",
                schema: "rag");
        }
    }
}
