using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MasaDurum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasaDurum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Masa",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MasaDurumId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_Masa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Masa_MasaDurum_MasaDurumId",
                        column: x => x.MasaDurumId,
                        principalTable: "MasaDurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Adisyon",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasaId = table.Column<long>(type: "bigint", nullable: false),
                    AcanKullaniciId = table.Column<long>(type: "bigint", nullable: false),
                    AcanCihazId = table.Column<long>(type: "bigint", nullable: false),
                    AcilisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KapanisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Durum = table.Column<short>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_Adisyon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adisyon_Cihaz_AcanCihazId",
                        column: x => x.AcanCihazId,
                        principalTable: "Cihaz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Adisyon_Kullanici_AcanKullaniciId",
                        column: x => x.AcanKullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Adisyon_Masa_MasaId",
                        column: x => x.MasaId,
                        principalTable: "Masa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MasaDurum",
                columns: new[] { "Id", "Ad", "AktifMi" },
                values: new object[,]
                {
                    { 1L, "Bos", true },
                    { 2L, "Dolu", true },
                    { 3L, "Rezerve", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adisyon_AcanCihazId",
                table: "Adisyon",
                column: "AcanCihazId");

            migrationBuilder.CreateIndex(
                name: "IX_Adisyon_AcanKullaniciId",
                table: "Adisyon",
                column: "AcanKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Adisyon_MasaId_Durum",
                table: "Adisyon",
                columns: new[] { "MasaId", "Durum" });

            migrationBuilder.CreateIndex(
                name: "IX_Adisyon_TenantId_SubeId_AcilisTarihi",
                table: "Adisyon",
                columns: new[] { "TenantId", "SubeId", "AcilisTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Masa_MasaDurumId",
                table: "Masa",
                column: "MasaDurumId");

            migrationBuilder.CreateIndex(
                name: "IX_Masa_TenantId_SubeId_SilindiMi",
                table: "Masa",
                columns: new[] { "TenantId", "SubeId", "SilindiMi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adisyon");

            migrationBuilder.DropTable(
                name: "Masa");

            migrationBuilder.DropTable(
                name: "MasaDurum");
        }
    }
}
