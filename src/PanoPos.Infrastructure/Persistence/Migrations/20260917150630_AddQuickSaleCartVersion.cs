using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuickSaleCartVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FiyatTipiId",
                table: "Siparis",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Surum",
                table: "Siparis",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Siparis_FiyatTipiId",
                table: "Siparis",
                column: "FiyatTipiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Siparis_FiyatTipi_FiyatTipiId",
                table: "Siparis",
                column: "FiyatTipiId",
                principalTable: "FiyatTipi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Siparis_FiyatTipi_FiyatTipiId",
                table: "Siparis");

            migrationBuilder.DropIndex(
                name: "IX_Siparis_FiyatTipiId",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "FiyatTipiId",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "Surum",
                table: "Siparis");
        }
    }
}
