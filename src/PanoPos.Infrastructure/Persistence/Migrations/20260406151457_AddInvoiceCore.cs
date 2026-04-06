using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fatura",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SiparisId = table.Column<long>(type: "bigint", nullable: true),
                    CariId = table.Column<long>(type: "bigint", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ToplamTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Durum = table.Column<short>(type: "smallint", nullable: false),
                    KapanisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KapatanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_Fatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fatura_Cari_CariId",
                        column: x => x.CariId,
                        principalTable: "Cari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fatura_Siparis_SiparisId",
                        column: x => x.SiparisId,
                        principalTable: "Siparis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaturaDetay",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaId = table.Column<long>(type: "bigint", nullable: false),
                    UrunId = table.Column<long>(type: "bigint", nullable: false),
                    UrunVaryantId = table.Column<long>(type: "bigint", nullable: true),
                    Miktar = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatirToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_FaturaDetay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturaDetay_Fatura_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Fatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaturaDetay_UrunVaryant_UrunVaryantId",
                        column: x => x.UrunVaryantId,
                        principalTable: "UrunVaryant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaturaDetay_Urun_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_CariId",
                table: "Fatura",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_SiparisId",
                table: "Fatura",
                column: "SiparisId");

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_TenantId_FaturaNo",
                table: "Fatura",
                columns: new[] { "TenantId", "FaturaNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_TenantId_SubeId_Durum",
                table: "Fatura",
                columns: new[] { "TenantId", "SubeId", "Durum" });

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetay_FaturaId",
                table: "FaturaDetay",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetay_UrunId",
                table: "FaturaDetay",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetay_UrunVaryantId",
                table: "FaturaDetay",
                column: "UrunVaryantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaturaDetay");

            migrationBuilder.DropTable(
                name: "Fatura");
        }
    }
}
