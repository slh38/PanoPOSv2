using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkPurchaseInvoiceToStockLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StokHareket_Miktar",
                table: "StokHareket");

            migrationBuilder.AddColumn<long>(
                name: "AlisFaturaId",
                table: "StokFis",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DepoId",
                table: "AlisFatura",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StokHareket_Miktar",
                table: "StokHareket",
                sql: "[Miktar] <> 0 AND ([StokHareketTipi] <> 3 OR [Miktar] < 0) AND ([StokHareketTipi] NOT IN (1,4,5) OR [Miktar] > 0)");

            migrationBuilder.CreateIndex(
                name: "IX_StokFis_AlisFaturaId",
                table: "StokFis",
                column: "AlisFaturaId",
                unique: true,
                filter: "[AlisFaturaId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StokFis_AlisFatura",
                table: "StokFis",
                sql: "[AlisFaturaId] IS NULL OR [StokFisTipi] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFatura_DepoId",
                table: "AlisFatura",
                column: "DepoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFatura_Depo_DepoId",
                table: "AlisFatura",
                column: "DepoId",
                principalTable: "Depo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StokFis_AlisFatura_AlisFaturaId",
                table: "StokFis",
                column: "AlisFaturaId",
                principalTable: "AlisFatura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisFatura_Depo_DepoId",
                table: "AlisFatura");

            migrationBuilder.DropForeignKey(
                name: "FK_StokFis_AlisFatura_AlisFaturaId",
                table: "StokFis");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StokHareket_Miktar",
                table: "StokHareket");

            migrationBuilder.DropIndex(
                name: "IX_StokFis_AlisFaturaId",
                table: "StokFis");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StokFis_AlisFatura",
                table: "StokFis");

            migrationBuilder.DropIndex(
                name: "IX_AlisFatura_DepoId",
                table: "AlisFatura");

            migrationBuilder.DropColumn(
                name: "AlisFaturaId",
                table: "StokFis");

            migrationBuilder.DropColumn(
                name: "DepoId",
                table: "AlisFatura");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StokHareket_Miktar",
                table: "StokHareket",
                sql: "[Miktar] <> 0 AND ([StokHareketTipi] <> 3 OR [Miktar] < 0) AND ([StokHareketTipi] NOT IN (1,4) OR [Miktar] > 0)");
        }
    }
}
