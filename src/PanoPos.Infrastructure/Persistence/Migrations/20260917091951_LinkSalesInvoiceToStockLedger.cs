using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkSalesInvoiceToStockLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StokHareket_Miktar",
                table: "StokHareket");

            migrationBuilder.AlterColumn<string>(
                name: "BirimKodu",
                table: "StokFisDetay",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<long>(
                name: "FaturaId",
                table: "StokFis",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BirimKodu",
                table: "SiparisDetay",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BirimKodu",
                table: "FaturaDetay",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DepoId",
                table: "Fatura",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StokHareket_Miktar",
                table: "StokHareket",
                sql: "[Miktar] <> 0 AND ([StokHareketTipi] NOT IN (3,6) OR [Miktar] < 0) AND ([StokHareketTipi] NOT IN (1,4,5) OR [Miktar] > 0)");

            migrationBuilder.CreateIndex(
                name: "IX_StokFis_FaturaId",
                table: "StokFis",
                column: "FaturaId",
                unique: true,
                filter: "[FaturaId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StokFis_Fatura",
                table: "StokFis",
                sql: "[FaturaId] IS NULL OR ([StokFisTipi] = 2 AND [AlisFaturaId] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_DepoId",
                table: "Fatura",
                column: "DepoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fatura_Depo_DepoId",
                table: "Fatura",
                column: "DepoId",
                principalTable: "Depo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StokFis_Fatura_FaturaId",
                table: "StokFis",
                column: "FaturaId",
                principalTable: "Fatura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fatura_Depo_DepoId",
                table: "Fatura");

            migrationBuilder.DropForeignKey(
                name: "FK_StokFis_Fatura_FaturaId",
                table: "StokFis");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StokHareket_Miktar",
                table: "StokHareket");

            migrationBuilder.DropIndex(
                name: "IX_StokFis_FaturaId",
                table: "StokFis");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StokFis_Fatura",
                table: "StokFis");

            migrationBuilder.DropIndex(
                name: "IX_Fatura_DepoId",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "FaturaId",
                table: "StokFis");

            migrationBuilder.DropColumn(
                name: "BirimKodu",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "BirimKodu",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "DepoId",
                table: "Fatura");

            migrationBuilder.AlterColumn<string>(
                name: "BirimKodu",
                table: "StokFisDetay",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StokHareket_Miktar",
                table: "StokHareket",
                sql: "[Miktar] <> 0 AND ([StokHareketTipi] <> 3 OR [Miktar] < 0) AND ([StokHareketTipi] NOT IN (1,4,5) OR [Miktar] > 0)");
        }
    }
}
