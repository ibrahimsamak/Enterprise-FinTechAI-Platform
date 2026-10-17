using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinAI.Modules.Fraud.Migrations
{
    public partial class InitialFraud : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fraud");

            migrationBuilder.CreateTable(
                name: "audit_records",
                schema: "fraud",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    AtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                schema: "fraud",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    Reasons = table.Column<List<string>>(type: "text[]", nullable: false),
                    Investigation = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "fraud",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    ClrType = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    TraceParent = table.Column<string>(type: "text", nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_records_PaymentId",
                schema: "fraud",
                table: "audit_records",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_PaymentId",
                schema: "fraud",
                table: "Cases",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedOnUtc",
                schema: "fraud",
                table: "outbox_messages",
                column: "ProcessedOnUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_records",
                schema: "fraud");

            migrationBuilder.DropTable(
                name: "Cases",
                schema: "fraud");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "fraud");
        }
    }
}
