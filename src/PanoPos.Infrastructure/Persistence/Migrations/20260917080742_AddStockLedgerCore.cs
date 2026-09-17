using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockLedgerCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StokFis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokFisTipi = table.Column<int>(type: "int", nullable: false),
                    FisNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepoId = table.Column<long>(type: "bigint", nullable: true),
                    KaynakDepoId = table.Column<long>(type: "bigint", nullable: true),
                    HedefDepoId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_StokFis", x => x.Id);
                    table.CheckConstraint("CK_StokFis_Depo", "([StokFisTipi] = 7 AND [DepoId] IS NULL AND [KaynakDepoId] IS NOT NULL AND [HedefDepoId] IS NOT NULL AND [KaynakDepoId] <> [HedefDepoId]) OR ([StokFisTipi] <> 7 AND [DepoId] IS NOT NULL AND [KaynakDepoId] IS NULL AND [HedefDepoId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_StokFis_Depo_DepoId",
                        column: x => x.DepoId,
                        principalTable: "Depo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokFis_Depo_HedefDepoId",
                        column: x => x.HedefDepoId,
                        principalTable: "Depo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokFis_Depo_KaynakDepoId",
                        column: x => x.KaynakDepoId,
                        principalTable: "Depo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokFis_Sube_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Sube",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokFis_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StokFisDetay",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokFisId = table.Column<long>(type: "bigint", nullable: false),
                    StokKartId = table.Column<long>(type: "bigint", nullable: false),
                    StokKartVaryantId = table.Column<long>(type: "bigint", nullable: true),
                    StokKartSatisBirimiId = table.Column<long>(type: "bigint", nullable: false),
                    BirimKodu = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BirimAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Katsayi = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SistemMiktari = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SayilanMiktar = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    FarkMiktari = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
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
                    table.PrimaryKey("PK_StokFisDetay", x => x.Id);
                    table.UniqueConstraint("AK_StokFisDetay_Id_StokFisId", x => new { x.Id, x.StokFisId });
                    table.ForeignKey(
                        name: "FK_StokFisDetay_StokFis_StokFisId",
                        column: x => x.StokFisId,
                        principalTable: "StokFis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokFisDetay_StokKartSatisBirimi_StokKartSatisBirimiId",
                        column: x => x.StokKartSatisBirimiId,
                        principalTable: "StokKartSatisBirimi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokFisDetay_StokKartVaryant_StokKartVaryantId",
                        column: x => x.StokKartVaryantId,
                        principalTable: "StokKartVaryant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokFisDetay_StokKart_StokKartId",
                        column: x => x.StokKartId,
                        principalTable: "StokKart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StokHareket",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    DepoId = table.Column<long>(type: "bigint", nullable: false),
                    StokKartId = table.Column<long>(type: "bigint", nullable: false),
                    StokKartVaryantId = table.Column<long>(type: "bigint", nullable: true),
                    StokFisId = table.Column<long>(type: "bigint", nullable: false),
                    StokFisDetayId = table.Column<long>(type: "bigint", nullable: false),
                    StokHareketTipi = table.Column<int>(type: "int", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    HareketTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokHareket", x => x.Id);
                    table.CheckConstraint("CK_StokHareket_Miktar", "[Miktar] <> 0 AND ([StokHareketTipi] <> 3 OR [Miktar] < 0) AND ([StokHareketTipi] NOT IN (1,4) OR [Miktar] > 0)");
                    table.ForeignKey(
                        name: "FK_StokHareket_Depo_DepoId",
                        column: x => x.DepoId,
                        principalTable: "Depo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokHareket_StokFisDetay_StokFisDetayId_StokFisId",
                        columns: x => new { x.StokFisDetayId, x.StokFisId },
                        principalTable: "StokFisDetay",
                        principalColumns: new[] { "Id", "StokFisId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokHareket_StokFis_StokFisId",
                        column: x => x.StokFisId,
                        principalTable: "StokFis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokHareket_StokKartVaryant_StokKartVaryantId",
                        column: x => x.StokKartVaryantId,
                        principalTable: "StokKartVaryant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokHareket_StokKart_StokKartId",
                        column: x => x.StokKartId,
                        principalTable: "StokKart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokHareket_Sube_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Sube",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokHareket_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StokFis_DepoId",
                table: "StokFis",
                column: "DepoId");

            migrationBuilder.CreateIndex(
                name: "IX_StokFis_HedefDepoId",
                table: "StokFis",
                column: "HedefDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_StokFis_KaynakDepoId",
                table: "StokFis",
                column: "KaynakDepoId");

            migrationBuilder.CreateIndex(
                name: "IX_StokFis_SubeId",
                table: "StokFis",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_StokFis_TenantId_FisNo",
                table: "StokFis",
                columns: new[] { "TenantId", "FisNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokFis_TenantId_SubeId_FisTarihi_StokFisTipi",
                table: "StokFis",
                columns: new[] { "TenantId", "SubeId", "FisTarihi", "StokFisTipi" });

            migrationBuilder.CreateIndex(
                name: "IX_StokFisDetay_StokFisId",
                table: "StokFisDetay",
                column: "StokFisId");

            migrationBuilder.CreateIndex(
                name: "IX_StokFisDetay_StokKartId",
                table: "StokFisDetay",
                column: "StokKartId");

            migrationBuilder.CreateIndex(
                name: "IX_StokFisDetay_StokKartSatisBirimiId",
                table: "StokFisDetay",
                column: "StokKartSatisBirimiId");

            migrationBuilder.CreateIndex(
                name: "IX_StokFisDetay_StokKartVaryantId",
                table: "StokFisDetay",
                column: "StokKartVaryantId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareket_DepoId",
                table: "StokHareket",
                column: "DepoId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareket_StokFisDetayId_StokFisId",
                table: "StokHareket",
                columns: new[] { "StokFisDetayId", "StokFisId" });

            migrationBuilder.CreateIndex(
                name: "IX_StokHareket_StokFisDetayId_StokHareketTipi",
                table: "StokHareket",
                columns: new[] { "StokFisDetayId", "StokHareketTipi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokHareket_StokFisId",
                table: "StokHareket",
                column: "StokFisId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareket_StokKartId",
                table: "StokHareket",
                column: "StokKartId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareket_StokKartVaryantId",
                table: "StokHareket",
                column: "StokKartVaryantId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareket_SubeId",
                table: "StokHareket",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareket_TenantId_SubeId_DepoId_StokKartId_StokKartVaryantId",
                table: "StokHareket",
                columns: new[] { "TenantId", "SubeId", "DepoId", "StokKartId", "StokKartVaryantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StokHareket");

            migrationBuilder.DropTable(
                name: "StokFisDetay");

            migrationBuilder.DropTable(
                name: "StokFis");
        }
    }
}
