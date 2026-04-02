using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantBarcodeCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beden",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_Beden", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Renk",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_Renk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Urun",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UrunTipi = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Urun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunVaryant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunId = table.Column<long>(type: "bigint", nullable: false),
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
                name: "Barkod",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarkodNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BarkodTipi = table.Column<int>(type: "int", nullable: false),
                    UrunId = table.Column<long>(type: "bigint", nullable: true),
                    UrunVaryantId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_Barkod", x => x.Id);
                    table.CheckConstraint("CK_Barkod_Hedef", "([UrunId] IS NOT NULL AND [UrunVaryantId] IS NULL) OR ([UrunId] IS NULL AND [UrunVaryantId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Barkod_UrunVaryant_UrunVaryantId",
                        column: x => x.UrunVaryantId,
                        principalTable: "UrunVaryant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Barkod_Urun_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Barkod_TenantId_BarkodNo",
                table: "Barkod",
                columns: new[] { "TenantId", "BarkodNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Barkod_UrunId",
                table: "Barkod",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_Barkod_UrunVaryantId",
                table: "Barkod",
                column: "UrunVaryantId");

            migrationBuilder.CreateIndex(
                name: "IX_Beden_TenantId_Ad",
                table: "Beden",
                columns: new[] { "TenantId", "Ad" });

            migrationBuilder.CreateIndex(
                name: "IX_Beden_TenantId_Kod",
                table: "Beden",
                columns: new[] { "TenantId", "Kod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Renk_TenantId_Ad",
                table: "Renk",
                columns: new[] { "TenantId", "Ad" });

            migrationBuilder.CreateIndex(
                name: "IX_Renk_TenantId_Kod",
                table: "Renk",
                columns: new[] { "TenantId", "Kod" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Barkod");

            migrationBuilder.DropTable(
                name: "UrunVaryant");

            migrationBuilder.DropTable(
                name: "Beden");

            migrationBuilder.DropTable(
                name: "Renk");

            migrationBuilder.DropTable(
                name: "Urun");
        }
    }
}
