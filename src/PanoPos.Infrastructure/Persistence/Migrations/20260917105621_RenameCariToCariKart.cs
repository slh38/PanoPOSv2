using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameCariToCariKart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisFatura_Cari_CariId",
                table: "AlisFatura");

            migrationBuilder.DropForeignKey(
                name: "FK_CariHareket_Cari_CariId",
                table: "CariHareket");

            migrationBuilder.DropForeignKey(
                name: "FK_Fatura_Cari_CariId",
                table: "Fatura");

            migrationBuilder.DropForeignKey(
                name: "FK_Siparis_Cari_CariId",
                table: "Siparis");

            migrationBuilder.DropPrimaryKey(name: "PK_Cari", table: "Cari");

            migrationBuilder.RenameTable(name: "Cari", newName: "CariKart");

            migrationBuilder.RenameIndex(
                name: "IX_Cari_TenantId_CariKodu",
                table: "CariKart",
                newName: "IX_CariKart_TenantId_CariKodu");

            migrationBuilder.RenameIndex(
                name: "IX_Cari_TenantId_SubeId_SilindiMi",
                table: "CariKart",
                newName: "IX_CariKart_TenantId_SubeId_SilindiMi");

            migrationBuilder.AddPrimaryKey(name: "PK_CariKart", table: "CariKart", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFatura_CariKart_CariId",
                table: "AlisFatura",
                column: "CariId",
                principalTable: "CariKart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareket_CariKart_CariId",
                table: "CariHareket",
                column: "CariId",
                principalTable: "CariKart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fatura_CariKart_CariId",
                table: "Fatura",
                column: "CariId",
                principalTable: "CariKart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Siparis_CariKart_CariId",
                table: "Siparis",
                column: "CariId",
                principalTable: "CariKart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisFatura_CariKart_CariId",
                table: "AlisFatura");

            migrationBuilder.DropForeignKey(
                name: "FK_CariHareket_CariKart_CariId",
                table: "CariHareket");

            migrationBuilder.DropForeignKey(
                name: "FK_Fatura_CariKart_CariId",
                table: "Fatura");

            migrationBuilder.DropForeignKey(
                name: "FK_Siparis_CariKart_CariId",
                table: "Siparis");

            migrationBuilder.DropPrimaryKey(name: "PK_CariKart", table: "CariKart");

            migrationBuilder.RenameTable(name: "CariKart", newName: "Cari");

            migrationBuilder.RenameIndex(
                name: "IX_CariKart_TenantId_CariKodu",
                table: "Cari",
                newName: "IX_Cari_TenantId_CariKodu");

            migrationBuilder.RenameIndex(
                name: "IX_CariKart_TenantId_SubeId_SilindiMi",
                table: "Cari",
                newName: "IX_Cari_TenantId_SubeId_SilindiMi");

            migrationBuilder.AddPrimaryKey(name: "PK_Cari", table: "Cari", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFatura_Cari_CariId",
                table: "AlisFatura",
                column: "CariId",
                principalTable: "Cari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareket_Cari_CariId",
                table: "CariHareket",
                column: "CariId",
                principalTable: "Cari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fatura_Cari_CariId",
                table: "Fatura",
                column: "CariId",
                principalTable: "Cari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Siparis_Cari_CariId",
                table: "Siparis",
                column: "CariId",
                principalTable: "Cari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
