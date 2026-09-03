using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductDomainToStockCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barkod_UrunSatisBirimi_UrunSatisBirimiId",
                table: "Barkod");

            migrationBuilder.DropForeignKey(
                name: "FK_Barkod_UrunVaryant_UrunVaryantId",
                table: "Barkod");

            migrationBuilder.DropForeignKey(
                name: "FK_Barkod_Urun_UrunId",
                table: "Barkod");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_UrunSatisBirimi_UrunSatisBirimiId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_UrunVaryant_UrunVaryantId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_Urun_UrunId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_UrunSatisBirimi_UrunSatisBirimiId",
                table: "SiparisDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_UrunVaryant_UrunVaryantId",
                table: "SiparisDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_Urun_UrunId",
                table: "SiparisDetay");

            migrationBuilder.DropTable(
                name: "UrunFiyat");

            migrationBuilder.DropTable(
                name: "UrunVaryant");

            migrationBuilder.DropTable(
                name: "UrunSatisBirimi");

            migrationBuilder.DropTable(
                name: "Urun");

            migrationBuilder.DropTable(
                name: "UrunGrup");

            migrationBuilder.DropTable(
                name: "UrunKategori");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Barkod_Hedef",
                table: "Barkod");

            migrationBuilder.RenameColumn(
                name: "UrunVaryantId",
                table: "SiparisDetay",
                newName: "StokKartVaryantId");

            migrationBuilder.RenameColumn(
                name: "UrunSatisBirimiId",
                table: "SiparisDetay",
                newName: "StokKartSatisBirimiId");

            migrationBuilder.RenameColumn(
                name: "UrunId",
                table: "SiparisDetay",
                newName: "StokKartId");

            migrationBuilder.RenameIndex(
                name: "IX_SiparisDetay_UrunVaryantId",
                table: "SiparisDetay",
                newName: "IX_SiparisDetay_StokKartVaryantId");

            migrationBuilder.RenameIndex(
                name: "IX_SiparisDetay_UrunSatisBirimiId",
                table: "SiparisDetay",
                newName: "IX_SiparisDetay_StokKartSatisBirimiId");

            migrationBuilder.RenameIndex(
                name: "IX_SiparisDetay_UrunId",
                table: "SiparisDetay",
                newName: "IX_SiparisDetay_StokKartId");

            migrationBuilder.RenameColumn(
                name: "UrunVaryantId",
                table: "FaturaDetay",
                newName: "StokKartVaryantId");

            migrationBuilder.RenameColumn(
                name: "UrunSatisBirimiId",
                table: "FaturaDetay",
                newName: "StokKartSatisBirimiId");

            migrationBuilder.RenameColumn(
                name: "UrunId",
                table: "FaturaDetay",
                newName: "StokKartId");

            migrationBuilder.RenameIndex(
                name: "IX_FaturaDetay_UrunVaryantId",
                table: "FaturaDetay",
                newName: "IX_FaturaDetay_StokKartVaryantId");

            migrationBuilder.RenameIndex(
                name: "IX_FaturaDetay_UrunSatisBirimiId",
                table: "FaturaDetay",
                newName: "IX_FaturaDetay_StokKartSatisBirimiId");

            migrationBuilder.RenameIndex(
                name: "IX_FaturaDetay_UrunId",
                table: "FaturaDetay",
                newName: "IX_FaturaDetay_StokKartId");

            migrationBuilder.RenameColumn(
                name: "UrunVaryantId",
                table: "Barkod",
                newName: "StokKartVaryantId");

            migrationBuilder.RenameColumn(
                name: "UrunSatisBirimiId",
                table: "Barkod",
                newName: "StokKartSatisBirimiId");

            migrationBuilder.RenameColumn(
                name: "UrunId",
                table: "Barkod",
                newName: "StokKartId");

            migrationBuilder.RenameIndex(
                name: "IX_Barkod_UrunVaryantId",
                table: "Barkod",
                newName: "IX_Barkod_StokKartVaryantId");

            migrationBuilder.RenameIndex(
                name: "IX_Barkod_UrunSatisBirimiId",
                table: "Barkod",
                newName: "IX_Barkod_StokKartSatisBirimiId");

            migrationBuilder.RenameIndex(
                name: "IX_Barkod_UrunId",
                table: "Barkod",
                newName: "IX_Barkod_StokKartId");

            migrationBuilder.CreateTable(
                name: "StokGrup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_StokGrup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StokKategori",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_StokKategori", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StokKart",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokKartKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StokKartTipi = table.Column<int>(type: "int", nullable: false),
                    StokKategoriId = table.Column<long>(type: "bigint", nullable: true),
                    StokGrupId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_StokKart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StokKart_StokGrup_StokGrupId",
                        column: x => x.StokGrupId,
                        principalTable: "StokGrup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokKart_StokKategori_StokKategoriId",
                        column: x => x.StokKategoriId,
                        principalTable: "StokKategori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StokKartSatisBirimi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokKartId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_StokKartSatisBirimi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StokKartSatisBirimi_StokKart_StokKartId",
                        column: x => x.StokKartId,
                        principalTable: "StokKart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StokKartVaryant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokKartId = table.Column<long>(type: "bigint", nullable: false),
                    RenkId = table.Column<long>(type: "bigint", nullable: true),
                    BedenId = table.Column<long>(type: "bigint", nullable: true),
                    VaryantKodu = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    BarkodluMu = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_StokKartVaryant", x => x.Id);
                    table.CheckConstraint("CK_StokKartVaryant_RenkVeyaBeden", "[RenkId] IS NOT NULL OR [BedenId] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_StokKartVaryant_Beden_BedenId",
                        column: x => x.BedenId,
                        principalTable: "Beden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokKartVaryant_Renk_RenkId",
                        column: x => x.RenkId,
                        principalTable: "Renk",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokKartVaryant_StokKart_StokKartId",
                        column: x => x.StokKartId,
                        principalTable: "StokKart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StokKartFiyat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokKartSatisBirimiId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_StokKartFiyat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StokKartFiyat_FiyatTipi_FiyatTipiId",
                        column: x => x.FiyatTipiId,
                        principalTable: "FiyatTipi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokKartFiyat_StokKartSatisBirimi_StokKartSatisBirimiId",
                        column: x => x.StokKartSatisBirimiId,
                        principalTable: "StokKartSatisBirimi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Barkod_Hedef",
                table: "Barkod",
                sql: "([StokKartId] IS NOT NULL AND [StokKartVaryantId] IS NULL) OR ([StokKartId] IS NULL AND [StokKartVaryantId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_StokGrup_TenantId_Kod",
                table: "StokGrup",
                columns: new[] { "TenantId", "Kod" },
                unique: true,
                filter: "[Kod] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StokGrup_TenantId_SubeId_SilindiMi",
                table: "StokGrup",
                columns: new[] { "TenantId", "SubeId", "SilindiMi" });

            migrationBuilder.CreateIndex(
                name: "IX_StokKart_StokGrupId",
                table: "StokKart",
                column: "StokGrupId");

            migrationBuilder.CreateIndex(
                name: "IX_StokKart_StokKategoriId",
                table: "StokKart",
                column: "StokKategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_StokKart_TenantId_StokKartKodu",
                table: "StokKart",
                columns: new[] { "TenantId", "StokKartKodu" },
                unique: true,
                filter: "[StokKartKodu] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StokKart_TenantId_SubeId_SilindiMi",
                table: "StokKart",
                columns: new[] { "TenantId", "SubeId", "SilindiMi" });

            migrationBuilder.CreateIndex(
                name: "IX_StokKartFiyat_FiyatTipiId",
                table: "StokKartFiyat",
                column: "FiyatTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_StokKartFiyat_StokKartSatisBirimiId_FiyatTipiId",
                table: "StokKartFiyat",
                columns: new[] { "StokKartSatisBirimiId", "FiyatTipiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokKartSatisBirimi_StokKartId_BirimKodu",
                table: "StokKartSatisBirimi",
                columns: new[] { "StokKartId", "BirimKodu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokKartVaryant_BedenId",
                table: "StokKartVaryant",
                column: "BedenId");

            migrationBuilder.CreateIndex(
                name: "IX_StokKartVaryant_RenkId",
                table: "StokKartVaryant",
                column: "RenkId");

            migrationBuilder.CreateIndex(
                name: "IX_StokKartVaryant_StokKartId",
                table: "StokKartVaryant",
                column: "StokKartId");

            migrationBuilder.CreateIndex(
                name: "IX_StokKartVaryant_TenantId_StokKartId_RenkId_BedenId",
                table: "StokKartVaryant",
                columns: new[] { "TenantId", "StokKartId", "RenkId", "BedenId" },
                unique: true,
                filter: "[RenkId] IS NOT NULL AND [BedenId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StokKategori_TenantId_Kod",
                table: "StokKategori",
                columns: new[] { "TenantId", "Kod" },
                unique: true,
                filter: "[Kod] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StokKategori_TenantId_SubeId_SilindiMi",
                table: "StokKategori",
                columns: new[] { "TenantId", "SubeId", "SilindiMi" });

            migrationBuilder.AddForeignKey(
                name: "FK_Barkod_StokKartSatisBirimi_StokKartSatisBirimiId",
                table: "Barkod",
                column: "StokKartSatisBirimiId",
                principalTable: "StokKartSatisBirimi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Barkod_StokKartVaryant_StokKartVaryantId",
                table: "Barkod",
                column: "StokKartVaryantId",
                principalTable: "StokKartVaryant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Barkod_StokKart_StokKartId",
                table: "Barkod",
                column: "StokKartId",
                principalTable: "StokKart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturaDetay_StokKartSatisBirimi_StokKartSatisBirimiId",
                table: "FaturaDetay",
                column: "StokKartSatisBirimiId",
                principalTable: "StokKartSatisBirimi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturaDetay_StokKartVaryant_StokKartVaryantId",
                table: "FaturaDetay",
                column: "StokKartVaryantId",
                principalTable: "StokKartVaryant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturaDetay_StokKart_StokKartId",
                table: "FaturaDetay",
                column: "StokKartId",
                principalTable: "StokKart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_StokKartSatisBirimi_StokKartSatisBirimiId",
                table: "SiparisDetay",
                column: "StokKartSatisBirimiId",
                principalTable: "StokKartSatisBirimi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_StokKartVaryant_StokKartVaryantId",
                table: "SiparisDetay",
                column: "StokKartVaryantId",
                principalTable: "StokKartVaryant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_StokKart_StokKartId",
                table: "SiparisDetay",
                column: "StokKartId",
                principalTable: "StokKart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barkod_StokKartSatisBirimi_StokKartSatisBirimiId",
                table: "Barkod");

            migrationBuilder.DropForeignKey(
                name: "FK_Barkod_StokKartVaryant_StokKartVaryantId",
                table: "Barkod");

            migrationBuilder.DropForeignKey(
                name: "FK_Barkod_StokKart_StokKartId",
                table: "Barkod");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_StokKartSatisBirimi_StokKartSatisBirimiId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_StokKartVaryant_StokKartVaryantId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturaDetay_StokKart_StokKartId",
                table: "FaturaDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_StokKartSatisBirimi_StokKartSatisBirimiId",
                table: "SiparisDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_StokKartVaryant_StokKartVaryantId",
                table: "SiparisDetay");

            migrationBuilder.DropForeignKey(
                name: "FK_SiparisDetay_StokKart_StokKartId",
                table: "SiparisDetay");

            migrationBuilder.DropTable(
                name: "StokKartFiyat");

            migrationBuilder.DropTable(
                name: "StokKartVaryant");

            migrationBuilder.DropTable(
                name: "StokKartSatisBirimi");

            migrationBuilder.DropTable(
                name: "StokKart");

            migrationBuilder.DropTable(
                name: "StokGrup");

            migrationBuilder.DropTable(
                name: "StokKategori");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Barkod_Hedef",
                table: "Barkod");

            migrationBuilder.RenameColumn(
                name: "StokKartVaryantId",
                table: "SiparisDetay",
                newName: "UrunVaryantId");

            migrationBuilder.RenameColumn(
                name: "StokKartSatisBirimiId",
                table: "SiparisDetay",
                newName: "UrunSatisBirimiId");

            migrationBuilder.RenameColumn(
                name: "StokKartId",
                table: "SiparisDetay",
                newName: "UrunId");

            migrationBuilder.RenameIndex(
                name: "IX_SiparisDetay_StokKartVaryantId",
                table: "SiparisDetay",
                newName: "IX_SiparisDetay_UrunVaryantId");

            migrationBuilder.RenameIndex(
                name: "IX_SiparisDetay_StokKartSatisBirimiId",
                table: "SiparisDetay",
                newName: "IX_SiparisDetay_UrunSatisBirimiId");

            migrationBuilder.RenameIndex(
                name: "IX_SiparisDetay_StokKartId",
                table: "SiparisDetay",
                newName: "IX_SiparisDetay_UrunId");

            migrationBuilder.RenameColumn(
                name: "StokKartVaryantId",
                table: "FaturaDetay",
                newName: "UrunVaryantId");

            migrationBuilder.RenameColumn(
                name: "StokKartSatisBirimiId",
                table: "FaturaDetay",
                newName: "UrunSatisBirimiId");

            migrationBuilder.RenameColumn(
                name: "StokKartId",
                table: "FaturaDetay",
                newName: "UrunId");

            migrationBuilder.RenameIndex(
                name: "IX_FaturaDetay_StokKartVaryantId",
                table: "FaturaDetay",
                newName: "IX_FaturaDetay_UrunVaryantId");

            migrationBuilder.RenameIndex(
                name: "IX_FaturaDetay_StokKartSatisBirimiId",
                table: "FaturaDetay",
                newName: "IX_FaturaDetay_UrunSatisBirimiId");

            migrationBuilder.RenameIndex(
                name: "IX_FaturaDetay_StokKartId",
                table: "FaturaDetay",
                newName: "IX_FaturaDetay_UrunId");

            migrationBuilder.RenameColumn(
                name: "StokKartVaryantId",
                table: "Barkod",
                newName: "UrunVaryantId");

            migrationBuilder.RenameColumn(
                name: "StokKartSatisBirimiId",
                table: "Barkod",
                newName: "UrunSatisBirimiId");

            migrationBuilder.RenameColumn(
                name: "StokKartId",
                table: "Barkod",
                newName: "UrunId");

            migrationBuilder.RenameIndex(
                name: "IX_Barkod_StokKartVaryantId",
                table: "Barkod",
                newName: "IX_Barkod_UrunVaryantId");

            migrationBuilder.RenameIndex(
                name: "IX_Barkod_StokKartSatisBirimiId",
                table: "Barkod",
                newName: "IX_Barkod_UrunSatisBirimiId");

            migrationBuilder.RenameIndex(
                name: "IX_Barkod_StokKartId",
                table: "Barkod",
                newName: "IX_Barkod_UrunId");

            migrationBuilder.CreateTable(
                name: "UrunGrup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunGrup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunKategori",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunKategori", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Urun",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunGrupId = table.Column<long>(type: "bigint", nullable: true),
                    UrunKategoriId = table.Column<long>(type: "bigint", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrunKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UrunTipi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Urun_UrunGrup_UrunGrupId",
                        column: x => x.UrunGrupId,
                        principalTable: "UrunGrup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Urun_UrunKategori_UrunKategoriId",
                        column: x => x.UrunKategoriId,
                        principalTable: "UrunKategori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UrunSatisBirimi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunId = table.Column<long>(type: "bigint", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    BirimAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BirimKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    Katsayi = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VarsayilanMi = table.Column<bool>(type: "bit", nullable: false)
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
                name: "UrunVaryant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BedenId = table.Column<long>(type: "bigint", nullable: true),
                    RenkId = table.Column<long>(type: "bigint", nullable: true),
                    UrunId = table.Column<long>(type: "bigint", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    BarkodluMu = table.Column<bool>(type: "bit", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaryantKodu = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunVaryant", x => x.Id);
                    table.CheckConstraint("CK_UrunVaryant_RenkVeyaBeden", "[RenkId] IS NOT NULL OR [BedenId] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_UrunVaryant_Beden_BedenId",
                        column: x => x.BedenId,
                        principalTable: "Beden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UrunVaryant_Renk_RenkId",
                        column: x => x.RenkId,
                        principalTable: "Renk",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UrunVaryant_Urun_UrunId",
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
                    FiyatTipiId = table.Column<long>(type: "bigint", nullable: false),
                    UrunSatisBirimiId = table.Column<long>(type: "bigint", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    Fiyat = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncelleyenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturanKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParaBirimKodu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SilenKullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    SilindiMi = table.Column<bool>(type: "bit", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

            migrationBuilder.AddCheckConstraint(
                name: "CK_Barkod_Hedef",
                table: "Barkod",
                sql: "([UrunId] IS NOT NULL AND [UrunVaryantId] IS NULL) OR ([UrunId] IS NULL AND [UrunVaryantId] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Urun_TenantId_SubeId_SilindiMi",
                table: "Urun",
                columns: new[] { "TenantId", "SubeId", "SilindiMi" });

            migrationBuilder.CreateIndex(
                name: "IX_Urun_TenantId_UrunKodu",
                table: "Urun",
                columns: new[] { "TenantId", "UrunKodu" },
                unique: true,
                filter: "[UrunKodu] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Urun_UrunGrupId",
                table: "Urun",
                column: "UrunGrupId");

            migrationBuilder.CreateIndex(
                name: "IX_Urun_UrunKategoriId",
                table: "Urun",
                column: "UrunKategoriId");

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
                name: "IX_UrunGrup_TenantId_Kod",
                table: "UrunGrup",
                columns: new[] { "TenantId", "Kod" },
                unique: true,
                filter: "[Kod] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrunGrup_TenantId_SubeId_SilindiMi",
                table: "UrunGrup",
                columns: new[] { "TenantId", "SubeId", "SilindiMi" });

            migrationBuilder.CreateIndex(
                name: "IX_UrunKategori_TenantId_Kod",
                table: "UrunKategori",
                columns: new[] { "TenantId", "Kod" },
                unique: true,
                filter: "[Kod] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrunKategori_TenantId_SubeId_SilindiMi",
                table: "UrunKategori",
                columns: new[] { "TenantId", "SubeId", "SilindiMi" });

            migrationBuilder.CreateIndex(
                name: "IX_UrunSatisBirimi_UrunId_BirimKodu",
                table: "UrunSatisBirimi",
                columns: new[] { "UrunId", "BirimKodu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrunVaryant_BedenId",
                table: "UrunVaryant",
                column: "BedenId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunVaryant_RenkId",
                table: "UrunVaryant",
                column: "RenkId");

            migrationBuilder.CreateIndex(
                name: "IX_UrunVaryant_TenantId_UrunId_RenkId_BedenId",
                table: "UrunVaryant",
                columns: new[] { "TenantId", "UrunId", "RenkId", "BedenId" },
                unique: true,
                filter: "[RenkId] IS NOT NULL AND [BedenId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrunVaryant_UrunId",
                table: "UrunVaryant",
                column: "UrunId");

            migrationBuilder.AddForeignKey(
                name: "FK_Barkod_UrunSatisBirimi_UrunSatisBirimiId",
                table: "Barkod",
                column: "UrunSatisBirimiId",
                principalTable: "UrunSatisBirimi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Barkod_UrunVaryant_UrunVaryantId",
                table: "Barkod",
                column: "UrunVaryantId",
                principalTable: "UrunVaryant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Barkod_Urun_UrunId",
                table: "Barkod",
                column: "UrunId",
                principalTable: "Urun",
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
                name: "FK_FaturaDetay_UrunVaryant_UrunVaryantId",
                table: "FaturaDetay",
                column: "UrunVaryantId",
                principalTable: "UrunVaryant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturaDetay_Urun_UrunId",
                table: "FaturaDetay",
                column: "UrunId",
                principalTable: "Urun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_UrunSatisBirimi_UrunSatisBirimiId",
                table: "SiparisDetay",
                column: "UrunSatisBirimiId",
                principalTable: "UrunSatisBirimi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_UrunVaryant_UrunVaryantId",
                table: "SiparisDetay",
                column: "UrunVaryantId",
                principalTable: "UrunVaryant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SiparisDetay_Urun_UrunId",
                table: "SiparisDetay",
                column: "UrunId",
                principalTable: "Urun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
