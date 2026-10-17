using System;
using FinAI.AI.Rag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace FinAI.AI.Rag.Migrations
{
    [DbContext(typeof(RagDbContext))]
    [Migration("20261017073641_InitialRag")]
    partial class InitialRag
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("rag")
                .HasAnnotation("ProductVersion", "10.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "vector");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FinAI.AI.Rag.PolicyChunk", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Audience")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ChunkIndex")
                        .HasColumnType("integer");

                    b.Property<string>("ContentHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DocId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Vector>("Embedding")
                        .IsRequired()
                        .HasColumnType("vector(1536)");

                    b.Property<string>("Heading")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("IndexedOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Section")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Audience");

                    b.HasIndex("Embedding");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Embedding"), "hnsw");
                    NpgsqlIndexBuilderExtensions.HasOperators(b.HasIndex("Embedding"), new[] { "vector_cosine_ops" });

                    b.HasIndex("DocId", "ChunkIndex")
                        .IsUnique();

                    b.ToTable("policy_chunks", "rag");
                });
#pragma warning restore 612, 618
        }
    }
}
