using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVatAndPurchaseInvoiceCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "KdvId",
                table: "StokKart",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "GenelIndirimPayi",
                table: "SiparisDetay",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "KdvDahilMi",
                table: "SiparisDetay",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "KdvId",
                table: "SiparisDetay",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "KdvOrani",
                table: "SiparisDetay",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "KdvTutari",
                table: "SiparisDetay",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Matrah",
                table: "SiparisDetay",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "KdvDahilMi",
                table: "Siparis",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ToplamKdv",
                table: "Siparis",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToplamMatrah",
                table: "Siparis",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GenelIndirimPayi",
                table: "FaturaDetay",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "KdvDahilMi",
                table: "FaturaDetay",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "KdvId",
                table: "FaturaDetay",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "KdvOrani",
                table: "FaturaDetay",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "KdvTutari",
                table: "FaturaDetay",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Matrah",
                table: "FaturaDetay",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "KdvDahilMi",
                table: "Fatura",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ToplamKdv",
                table: "Fatura",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToplamMatrah",
                table: "Fatura",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AlisFatura",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CariId = table.Column<long>(type: "bigint", nullable: false),
                    FaturaNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FaturaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParaBirimKodu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Kur = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    KdvDahilMi = table.Column<bool>(type: "bit", nullable: false),
                    AraToplam = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GenelIndirimOrani = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    GenelIndirimTutari = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ToplamMatrah = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ToplamKdv = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetToplam = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AlisFatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisFatura_Cari_CariId",
                        column: x => x.CariId,
                        principalTable: "Cari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlisFatura_Sube_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Sube",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlisFatura_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kdv",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Oran = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_Kdv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kdv_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantAyar",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SatisFiyatlariKdvDahilMi = table.Column<bool>(type: "bit", nullable: false),
                    AlisFiyatlariKdvDahilMi = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_TenantAyar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantAyar_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlisFaturaDetay",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlisFaturaId = table.Column<long>(type: "bigint", nullable: false),
                    StokKartId = table.Column<long>(type: "bigint", nullable: false),
                    StokKartVaryantId = table.Column<long>(type: "bigint", nullable: true),
                    StokKartSatisBirimiId = table.Column<long>(type: "bigint", nullable: false),
                    BirimKodu = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BirimAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Katsayi = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FiyatParaBirimKodu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FiyatKur = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SatirAraToplam = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IndirimOrani = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IndirimTutari = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GenelIndirimPayi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    KdvId = table.Column<long>(type: "bigint", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    KdvDahilMi = table.Column<bool>(type: "bit", nullable: false),
                    Matrah = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    KdvTutari = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SatirNetToplam = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_AlisFaturaDetay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisFaturaDetay_AlisFatura_AlisFaturaId",
                        column: x => x.AlisFaturaId,
                        principalTable: "AlisFatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlisFaturaDetay_Kdv_KdvId",
                        column: x => x.KdvId,
                        principalTable: "Kdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlisFaturaDetay_StokKartSatisBirimi_StokKartSatisBirimiId",
                        column: x => x.StokKartSatisBirimiId,
                        principalTable: "StokKartSatisBirimi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlisFaturaDetay_StokKartVaryant_StokKartVaryantId",
                        column: x => x.StokKartVaryantId,
                        principalTable: "StokKartVaryant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlisFaturaDetay_StokKart_StokKartId",
                        column: x => x.StokKartId,
                        principalTable: "StokKart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Kdv",
                columns: new[] { "Id", "Ad", "AktifMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "Kod", "OlusturanKullaniciId", "OlusturmaTarihi", "Oran", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[,]
                {
                    { 1L, "%0 KDV", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "KDV0", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") },
                    { 2L, "%1 KDV", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "KDV1", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1m, null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") },
                    { 3L, "%10 KDV", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "KDV10", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), 10m, null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") },
                    { 4L, "%20 KDV", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "KDV20", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), 20m, null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "TenantAyar",
                columns: new[] { "Id", "AktifMi", "AlisFiyatlariKdvDahilMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "OlusturanKullaniciId", "OlusturmaTarihi", "SatisFiyatlariKdvDahilMi", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[] { 1L, true, false, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), true, null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") });

            // Preserve all existing monetary columns; historical VAT was not recorded.
            migrationBuilder.Sql(@"
INSERT INTO Kdv (TenantId, SubeId, Kod, Ad, Oran, AktifMi, SilindiMi, OlusturmaTarihi)
SELECT t.TenantId, t.SubeId, 'KDV0', '%0 KDV', 0, 1, 0, SYSUTCDATETIME()
FROM Tenant t WHERE NOT EXISTS (SELECT 1 FROM Kdv k WHERE k.TenantId=t.TenantId AND k.Oran=0);
UPDATE s SET KdvId=k.Id FROM StokKart s JOIN Kdv k ON k.TenantId=s.TenantId AND k.Oran=0;
UPDATE d SET KdvId=k.Id, KdvDahilMi=1, Matrah=d.SatirNetToplam
FROM SiparisDetay d JOIN Kdv k ON k.TenantId=d.TenantId AND k.Oran=0;
UPDATE d SET KdvId=k.Id, KdvDahilMi=1, Matrah=d.SatirNetToplam
FROM FaturaDetay d JOIN Kdv k ON k.TenantId=d.TenantId AND k.Oran=0;
UPDATE Siparis SET KdvDahilMi=1, ToplamMatrah=NetToplam;
UPDATE Fatura SET KdvDahilMi=1, ToplamMatrah=NetToplam;
");
            migrationBuilder.CreateIndex(
                name: "IX_StokKart_KdvId",
                table: "StokKart",
                column: "KdvId");

            migrationBuilder.CreateIndex(
                name: "IX_SiparisDetay_KdvId",
                table: "SiparisDetay",
                column: "KdvId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetay_KdvId",
                table: "FaturaDetay",
                column: "KdvId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFatura_CariId",
                table: "AlisFatura",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFatura_SubeId",
                table: "AlisFatura",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFatura_TenantId_CariId_FaturaNo",
                table: "AlisFatura",
                columns: new[] { "TenantId", "CariId", "FaturaNo" });

            migrationBuilder.CreateIndex(
                name: "IX_AlisFatura_TenantId_SubeId_FaturaTarihi",
                table: "AlisFatura",
                columns: new[] { "TenantId", "SubeId", "FaturaTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaDetay_AlisFaturaId",
                table: "AlisFaturaDetay",
                column: "AlisFaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaDetay_KdvId",
                table: "AlisFaturaDetay",
                column: "KdvId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaDetay_StokKartId",
                table: "AlisFaturaDetay",
                column: "StokKartId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaDetay_StokKartSatisBirimiId",
                table: "AlisFaturaDetay",
                column: "StokKartSatisBirimiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaDetay_StokKartVaryantId",
                table: "AlisFaturaDetay",
                column: "StokKartVaryantId");

            migrationBuilder.CreateIndex(
                name: "IX_Kdv_TenantId_Kod",
                table: "Kdv",
                columns: new[] { "TenantId", "Kod" },
                unique: true,
                filter: "[AktifMi] = 1 AND [SilindiMi] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Kdv_TenantId_Oran",
                table: "Kdv",
                columns: new[] { "TenantId", "Oran" },
                unique: true,
                filter: "[AktifMi] = 1 AND [SilindiMi] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAyar_TenantId",
                table: "TenantAyar",
                column: "TenantId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturaDetay_Kdv_KdvId",
                table: "FaturaDetay",
                column: "KdvId",
                principalTable: "Kdv",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_Kdv_KdvId",
                table: "SiparisDetay",
                column: "KdvId",
                principalTable: "Kdv",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StokKart_Kdv_KdvId",
                table: "StokKart",
                column: "KdvId",
                principalTable: "Kdv",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_Kdv_KdvId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_Kdv_KdvId",
                table: "SiparisDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_StokKart_Kdv_KdvId",
                table: "StokKart");

            migrationBuilder.DropTable(
                name: "AlisFaturaDetay");

            migrationBuilder.DropTable(
                name: "TenantAyar");

            migrationBuilder.DropTable(
                name: "AlisFatura");

            migrationBuilder.DropTable(
                name: "Kdv");

            migrationBuilder.DropIndex(
                name: "IX_StokKart_KdvId",
                table: "StokKart");

            migrationBuilder.DropIndex(
                name: "IX_SiparisDetay_KdvId",
                table: "SiparisDetay");

            migrationBuilder.DropIndex(
                name: "IX_FaturaDetay_KdvId",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "KdvId",
                table: "StokKart");

            migrationBuilder.DropColumn(
                name: "GenelIndirimPayi",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "KdvDahilMi",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "KdvId",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "KdvOrani",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "KdvTutari",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "Matrah",
                table: "SiparisDetay");

            migrationBuilder.DropColumn(
                name: "KdvDahilMi",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "ToplamKdv",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "ToplamMatrah",
                table: "Siparis");

            migrationBuilder.DropColumn(
                name: "GenelIndirimPayi",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "KdvDahilMi",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "KdvId",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "KdvOrani",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "KdvTutari",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "Matrah",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "KdvDahilMi",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "ToplamKdv",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "ToplamMatrah",
                table: "Fatura");
        }
    }
}
