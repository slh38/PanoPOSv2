using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RevisePaymentForPartialCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "KalanTutar",
                table: "Fatura",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OdenenTutar",
                table: "Fatura",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
UPDATE Fatura
SET OdenenTutar = COALESCE((
        SELECT SUM(Tutar)
        FROM Tahsilat
        WHERE Tahsilat.FaturaId = Fatura.Id
          AND Tahsilat.SilindiMi = 0
    ), 0),
    KalanTutar = NetToplam - COALESCE((
        SELECT SUM(Tutar)
        FROM Tahsilat
        WHERE Tahsilat.FaturaId = Fatura.Id
          AND Tahsilat.SilindiMi = 0
    ), 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KalanTutar",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "OdenenTutar",
                table: "Fatura");
        }
    }
}
