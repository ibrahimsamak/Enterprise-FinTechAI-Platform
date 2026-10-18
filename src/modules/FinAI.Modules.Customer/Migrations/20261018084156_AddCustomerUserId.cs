using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinAI.Modules.Customer.Migrations
{
    public partial class AddCustomerUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "customer",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                schema: "customer",
                table: "Customers",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "customer",
                table: "Customers");
        }
    }
}
