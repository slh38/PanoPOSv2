using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceReceiptSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StokKartAd",
                table: "FaturaDetay",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VaryantKodu",
                table: "FaturaDetay",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CihazId",
                table: "Fatura",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_CihazId",
                table: "Fatura",
                column: "CihazId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fatura_Cihaz_CihazId",
                table: "Fatura",
                column: "CihazId",
                principalTable: "Cihaz",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fatura_Cihaz_CihazId",
                table: "Fatura");

            migrationBuilder.DropIndex(
                name: "IX_Fatura_CihazId",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "StokKartAd",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "VaryantKodu",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "CihazId",
                table: "Fatura");
        }
    }
}
