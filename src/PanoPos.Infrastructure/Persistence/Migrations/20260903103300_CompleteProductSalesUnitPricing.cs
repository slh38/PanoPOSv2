using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteProductSalesUnitPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "BirimFiyat",
                table: "SiparisDetay",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "BirimAdi",
                table: "SiparisDetay",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BirimKatsayi",
                table: "SiparisDetay",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FiyatKur",
                table: "SiparisDetay",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FiyatParaBirimKodu",
                table: "SiparisDetay",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "UrunSatisBirimiId",
                table: "SiparisDetay",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BirimFiyat",
                table: "FaturaDetay",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "BirimAdi",
                table: "FaturaDetay",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BirimKatsayi",
                table: "FaturaDetay",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FiyatKur",
                table: "FaturaDetay",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FiyatParaBirimKodu",
                table: "FaturaDetay",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "UrunSatisBirimiId",
                table: "FaturaDetay",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UrunSatisBirimiId",
                table: "Barkod",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FiyatTipi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiyatTipi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunSatisBirimi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunId = table.Column<long>(type: "bigint", nullable: false),
                    BirimKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirimAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Katsayi = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    VarsayilanMi = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunSatisBirimi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunSatisBirimi_Urun_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UrunFiyat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunSatisBirimiId = table.Column<long>(type: "bigint", nullable: false),
                    FiyatTipiId = table.Column<long>(type: "bigint", nullable: false),
                    Fiyat = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ParaBirimKodu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunFiyat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrunFiyat_FiyatTipi_FiyatTipiId",
                        column: x => x.FiyatTipiId,
                        principalTable: "FiyatTipi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UrunFiyat_UrunSatisBirimi_UrunSatisBirimiId",
                        column: x => x.UrunSatisBirimiId,
                        principalTable: "UrunSatisBirimi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "FiyatTipi",
                columns: new[] { "Id", "Ad", "AktifMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "Kod", "OlusturanKullaniciId", "OlusturmaTarihi", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[,]
                {
                    { 1L, "Perakende", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "PERAKENDE", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") },
                    { 2L, "Kredi Karti", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "KREDIKARTI", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") },
                    { 3L, "Toptan", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "TOPTAN", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiparisDetay_UrunSatisBirimiId",
                table: "SiparisDetay",
                column: "UrunSatisBirimiId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetay_UrunSatisBirimiId",
                table: "FaturaDetay",
                column: "UrunSatisBirimiId");

            migrationBuilder.CreateIndex(
                name: "IX_Barkod_UrunSatisBirimiId",
                table: "Barkod",
                column: "UrunSatisBirimiId");

            migrationBuilder.CreateIndex(
                name: "IX_FiyatTipi_TenantId_Kod",
                table: "FiyatTipi",
                columns: new[] { "TenantId", "Kod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyat_FiyatTipiId",
                table: "UrunFiyat",
                column: "FiyatTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyat_UrunSatisBirimiId_FiyatTipiId",
                table: "UrunFiyat",
                columns: new[] { "UrunSatisBirimiId", "FiyatTipiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrunSatisBirimi_UrunId_BirimKodu",
                table: "UrunSatisBirimi",
                columns: new[] { "UrunId", "BirimKodu" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Barkod_UrunSatisBirimi_UrunSatisBirimiId",
                table: "Barkod",
                column: "UrunSatisBirimiId",
                principalTable: "UrunSatisBirimi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturaDetay_UrunSatisBirimi_UrunSatisBirimiId",
                table: "FaturaDetay",
                column: "UrunSatisBirimiId",
                principalTable: "UrunSatisBirimi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_UrunSatisBirimi_UrunSatisBirimiId",
                table: "SiparisDetay",
                column: "UrunSatisBirimiId",
                principalTable: "UrunSatisBirimi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barkod_UrunSatisBirimi_UrunSatisBirimiId",
                table: "Barkod");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_UrunSatisBirimi_UrunSatisBirimiId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_UrunSatisBirimi_UrunSatisBirimiId",
                table: "SiparisDetay");

            migrationBuilder.DropTable(
                name: "UrunFiyat");

            migrationBuilder.DropTable(
                name: "FiyatTipi");

            migrationBuilder.DropTable(
                name: "UrunSatisBirimi");

            migrationBuilder.DropIndex(
                name: "IX_SiparisDetay_UrunSatisBirimiId",
                table: "SiparisDetay");

            migrationBuilder.DropIndex(
                name: "IX_FaturaDetay_UrunSatisBirimiId",
                table: "FaturaDetay");

            migrationBuilder.DropIndex(
                name: "IX_Barkod_UrunSatisBirimiId",
                table: "Barkod");

            migrationBuilder.DropColumn(
                name: "BirimAdi",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "BirimKatsayi",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "FiyatKur",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "FiyatParaBirimKodu",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "UrunSatisBirimiId",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "BirimAdi",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "BirimKatsayi",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "FiyatKur",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "FiyatParaBirimKodu",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "UrunSatisBirimiId",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "UrunSatisBirimiId",
                table: "Barkod");

            migrationBuilder.AlterColumn<decimal>(
                name: "BirimFiyat",
                table: "SiparisDetay",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "BirimFiyat",
                table: "FaturaDetay",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");
        }
    }
}
