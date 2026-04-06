using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReviseInvoiceDiscountAndCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IndirimOrani",
                table: "FaturaDetay",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IndirimTutari",
                table: "FaturaDetay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SatirAraToplam",
                table: "FaturaDetay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SatirNetToplam",
                table: "FaturaDetay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AraToplam",
                table: "Fatura",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GenelIndirimOrani",
                table: "Fatura",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GenelIndirimTutari",
                table: "Fatura",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Kur",
                table: "Fatura",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetToplam",
                table: "Fatura",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ParaBirimKodu",
                table: "Fatura",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndirimOrani",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "IndirimTutari",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "SatirAraToplam",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "SatirNetToplam",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "AraToplam",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "GenelIndirimOrani",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "GenelIndirimTutari",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "Kur",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "NetToplam",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "ParaBirimKodu",
                table: "Fatura");
        }
    }
}
