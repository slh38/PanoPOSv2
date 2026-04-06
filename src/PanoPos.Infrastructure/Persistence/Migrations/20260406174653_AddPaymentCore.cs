using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banka",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_Banka", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tahsilat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaId = table.Column<long>(type: "bigint", nullable: false),
                    TahsilatFisNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OdemeTipi = table.Column<short>(type: "smallint", nullable: false),
                    ParaBirimKodu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Kur = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YerelTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TahsilatTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_Tahsilat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tahsilat_Fatura_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Fatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankaHareket",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankaId = table.Column<long>(type: "bigint", nullable: false),
                    FaturaId = table.Column<long>(type: "bigint", nullable: true),
                    TahsilatId = table.Column<long>(type: "bigint", nullable: true),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ParaBirimKodu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Kur = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    YerelTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HareketTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BankaHareket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankaHareket_Banka_BankaId",
                        column: x => x.BankaId,
                        principalTable: "Banka",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankaHareket_Fatura_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Fatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankaHareket_Tahsilat_TahsilatId",
                        column: x => x.TahsilatId,
                        principalTable: "Tahsilat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CariHareket",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CariId = table.Column<long>(type: "bigint", nullable: false),
                    FaturaId = table.Column<long>(type: "bigint", nullable: true),
                    TahsilatId = table.Column<long>(type: "bigint", nullable: true),
                    HareketTipi = table.Column<short>(type: "smallint", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ParaBirimKodu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Kur = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    YerelTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HareketTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CariHareket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CariHareket_Cari_CariId",
                        column: x => x.CariId,
                        principalTable: "Cari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CariHareket_Fatura_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Fatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CariHareket_Tahsilat_TahsilatId",
                        column: x => x.TahsilatId,
                        principalTable: "Tahsilat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banka_TenantId_SubeId_Kod",
                table: "Banka",
                columns: new[] { "TenantId", "SubeId", "Kod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankaHareket_BankaId",
                table: "BankaHareket",
                column: "BankaId");

            migrationBuilder.CreateIndex(
                name: "IX_BankaHareket_FaturaId",
                table: "BankaHareket",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_BankaHareket_TahsilatId",
                table: "BankaHareket",
                column: "TahsilatId");

            migrationBuilder.CreateIndex(
                name: "IX_BankaHareket_TenantId_SubeId_BankaId_HareketTarihi",
                table: "BankaHareket",
                columns: new[] { "TenantId", "SubeId", "BankaId", "HareketTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_CariHareket_CariId",
                table: "CariHareket",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareket_FaturaId",
                table: "CariHareket",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareket_TahsilatId",
                table: "CariHareket",
                column: "TahsilatId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareket_TenantId_SubeId_CariId_HareketTarihi",
                table: "CariHareket",
                columns: new[] { "TenantId", "SubeId", "CariId", "HareketTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Tahsilat_FaturaId",
                table: "Tahsilat",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tahsilat_TenantId_SubeId_TahsilatTarihi",
                table: "Tahsilat",
                columns: new[] { "TenantId", "SubeId", "TahsilatTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Tahsilat_TenantId_TahsilatFisNo",
                table: "Tahsilat",
                columns: new[] { "TenantId", "TahsilatFisNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankaHareket");

            migrationBuilder.DropTable(
                name: "CariHareket");

            migrationBuilder.DropTable(
                name: "Banka");

            migrationBuilder.DropTable(
                name: "Tahsilat");
        }
    }
}
