using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IslemAnahtari",
                table: "Tahsilat",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IstekOzeti",
                table: "Tahsilat",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tahsilat_TenantId_IslemAnahtari",
                table: "Tahsilat",
                columns: new[] { "TenantId", "IslemAnahtari" },
                unique: true,
                filter: "[IslemAnahtari] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tahsilat_TenantId_IslemAnahtari",
                table: "Tahsilat");

            migrationBuilder.DropColumn(
                name: "IslemAnahtari",
                table: "Tahsilat");

            migrationBuilder.DropColumn(
                name: "IstekOzeti",
                table: "Tahsilat");
        }
    }
}
