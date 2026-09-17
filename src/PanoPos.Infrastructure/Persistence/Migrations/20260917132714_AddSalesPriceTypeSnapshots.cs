using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesPriceTypeSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FiyatTipiAdi",
                table: "SiparisDetay",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FiyatTipiId",
                table: "SiparisDetay",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiyatTipiAdi",
                table: "FaturaDetay",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FiyatTipiId",
                table: "FaturaDetay",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiparisDetay_FiyatTipiId",
                table: "SiparisDetay",
                column: "FiyatTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetay_FiyatTipiId",
                table: "FaturaDetay",
                column: "FiyatTipiId");

            migrationBuilder.AddForeignKey(
                name: "FK_FaturaDetay_FiyatTipi_FiyatTipiId",
                table: "FaturaDetay",
                column: "FiyatTipiId",
                principalTable: "FiyatTipi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_FiyatTipi_FiyatTipiId",
                table: "SiparisDetay",
                column: "FiyatTipiId",
                principalTable: "FiyatTipi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_FiyatTipi_FiyatTipiId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_FiyatTipi_FiyatTipiId",
                table: "SiparisDetay");

            migrationBuilder.DropIndex(
                name: "IX_SiparisDetay_FiyatTipiId",
                table: "SiparisDetay");

            migrationBuilder.DropIndex(
                name: "IX_FaturaDetay_FiyatTipiId",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "FiyatTipiAdi",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "FiyatTipiId",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "FiyatTipiAdi",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "FiyatTipiId",
                table: "FaturaDetay");
        }
    }
}
