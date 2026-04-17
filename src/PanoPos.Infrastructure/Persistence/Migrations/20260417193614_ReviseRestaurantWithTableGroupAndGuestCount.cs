using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReviseRestaurantWithTableGroupAndGuestCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kapasite",
                table: "Masa",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MasaGrupId",
                table: "Masa",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KisiSayisi",
                table: "Adisyon",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MasaGrup",
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
                    table.PrimaryKey("PK_MasaGrup", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Masa_MasaGrupId",
                table: "Masa",
                column: "MasaGrupId");

            migrationBuilder.CreateIndex(
                name: "IX_MasaGrup_TenantId_SubeId_SilindiMi",
                table: "MasaGrup",
                columns: new[] { "TenantId", "SubeId", "SilindiMi" });

            migrationBuilder.AddForeignKey(
                name: "FK_Masa_MasaGrup_MasaGrupId",
                table: "Masa",
                column: "MasaGrupId",
                principalTable: "MasaGrup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Masa_MasaGrup_MasaGrupId",
                table: "Masa");

            migrationBuilder.DropTable(
                name: "MasaGrup");

            migrationBuilder.DropIndex(
                name: "IX_Masa_MasaGrupId",
                table: "Masa");

            migrationBuilder.DropColumn(
                name: "Kapasite",
                table: "Masa");

            migrationBuilder.DropColumn(
                name: "MasaGrupId",
                table: "Masa");

            migrationBuilder.DropColumn(
                name: "KisiSayisi",
                table: "Adisyon");
        }
    }
}
