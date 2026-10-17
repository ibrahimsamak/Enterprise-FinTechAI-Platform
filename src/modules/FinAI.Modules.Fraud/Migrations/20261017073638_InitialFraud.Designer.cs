using System;
using System.Collections.Generic;
using FinAI.Modules.Fraud.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinAI.Modules.Fraud.Migrations
{
    [DbContext(typeof(FraudDbContext))]
    [Migration("20261017073638_InitialFraud")]
    partial class InitialFraud
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("fraud")
                .HasAnnotation("ProductVersion", "10.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FinAI.BuildingBlocks.Infrastructure.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ClrType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Topic")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TraceParent")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ProcessedOnUtc");

                    b.ToTable("outbox_messages", "fraud");
                });

            modelBuilder.Entity("FinAI.Modules.Fraud.Domain.AuditRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ActorId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("AtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("PaymentId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PaymentId");

                    b.ToTable("audit_records", "fraud");
                });

            modelBuilder.Entity("FinAI.Modules.Fraud.Domain.FraudCase", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<string>("Investigation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("PaymentId")
                        .HasColumnType("uuid");

                    b.PrimitiveCollection<List<string>>("Reasons")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<Guid?>("ResolvedBy")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("ResolvedOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("Score")
                        .HasColumnType("double precision");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.ComplexProperty(typeof(Dictionary<string, object>), "Amount", "FinAI.Modules.Fraud.Domain.FraudCase.Amount#Money", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<decimal>("Amount")
                                .HasColumnType("numeric")
                                .HasColumnName("amount");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("currency");
                        });

                    b.HasKey("Id");

                    b.HasIndex("PaymentId")
                        .IsUnique();

                    b.ToTable("Cases", "fraud");
                });
#pragma warning restore 612, 618
        }
    }
}
