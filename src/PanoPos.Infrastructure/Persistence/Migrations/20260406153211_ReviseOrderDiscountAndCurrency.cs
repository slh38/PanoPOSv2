using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReviseOrderDiscountAndCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IndirimOrani",
                table: "SiparisDetay",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IndirimTutari",
                table: "SiparisDetay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SatirAraToplam",
                table: "SiparisDetay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SatirNetToplam",
                table: "SiparisDetay",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AraToplam",
                table: "Siparis",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GenelIndirimOrani",
                table: "Siparis",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GenelIndirimTutari",
                table: "Siparis",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Kur",
                table: "Siparis",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetToplam",
                table: "Siparis",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ParaBirimKodu",
                table: "Siparis",
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
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "IndirimTutari",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "SatirAraToplam",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "SatirNetToplam",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "AraToplam",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "GenelIndirimOrani",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "GenelIndirimTutari",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "Kur",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "NetToplam",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "ParaBirimKodu",
                table: "Siparis");
        }
    }
}
