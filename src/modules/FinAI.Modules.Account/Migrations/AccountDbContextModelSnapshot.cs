using System;
using System.Collections.Generic;
using FinAI.Modules.Account.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinAI.Modules.Account.Migrations
{
    [DbContext(typeof(AccountDbContext))]
    partial class AccountDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("account")
                .HasAnnotation("ProductVersion", "10.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FinAI.Modules.Account.Domain.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.ComplexProperty(typeof(Dictionary<string, object>), "Balance", "FinAI.Modules.Account.Domain.Account.Balance#Money", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<decimal>("Amount")
                                .HasColumnType("numeric")
                                .HasColumnName("balance_amount");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("balance_currency");
                        });

                    b.HasKey("Id");

                    b.ToTable("Accounts", "account");
                });

            modelBuilder.Entity("FinAI.Modules.Account.Infrastructure.ReadModels.CustomerTransaction", b =>
                {
                    b.Property<Guid>("PaymentId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("PaymentId");

                    b.HasIndex("CustomerId", "OccurredOnUtc");

                    b.ToTable("customer_transactions", "account");
                });
#pragma warning restore 612, 618
        }
    }
}
