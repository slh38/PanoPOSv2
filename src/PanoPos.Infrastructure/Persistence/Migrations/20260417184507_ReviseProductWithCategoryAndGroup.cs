using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReviseProductWithCategoryAndGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UrunGrupId",
                table: "Urun",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UrunKategoriId",
                table: "Urun",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UrunGrup",
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
                    table.PrimaryKey("PK_UrunGrup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrunKategori",
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
                    table.PrimaryKey("PK_UrunKategori", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Urun_UrunGrupId",
                table: "Urun",
                column: "UrunGrupId");

            migrationBuilder.CreateIndex(
                name: "IX_Urun_UrunKategoriId",
                table: "Urun",
                column: "UrunKategoriId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Urun_UrunGrup_UrunGrupId",
                table: "Urun",
                column: "UrunGrupId",
                principalTable: "UrunGrup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Urun_UrunKategori_UrunKategoriId",
                table: "Urun",
                column: "UrunKategoriId",
                principalTable: "UrunKategori",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urun_UrunGrup_UrunGrupId",
                table: "Urun");

            migrationBuilder.DropForeignKey(
                name: "FK_Urun_UrunKategori_UrunKategoriId",
                table: "Urun");

            migrationBuilder.DropTable(
                name: "UrunGrup");

            migrationBuilder.DropTable(
                name: "UrunKategori");

            migrationBuilder.DropIndex(
                name: "IX_Urun_UrunGrupId",
                table: "Urun");

            migrationBuilder.DropIndex(
                name: "IX_Urun_UrunKategoriId",
                table: "Urun");

            migrationBuilder.DropColumn(
                name: "UrunGrupId",
                table: "Urun");

            migrationBuilder.DropColumn(
                name: "UrunKategoriId",
                table: "Urun");
        }
    }
}
