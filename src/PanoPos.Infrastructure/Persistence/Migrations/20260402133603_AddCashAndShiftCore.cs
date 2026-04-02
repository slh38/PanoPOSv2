using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCashAndShiftCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VarsayilanKasaId",
                table: "Cihaz",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Kasa",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_Kasa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vardiya",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KasaId = table.Column<long>(type: "bigint", nullable: false),
                    CihazId = table.Column<long>(type: "bigint", nullable: false),
                    KullaniciId = table.Column<long>(type: "bigint", nullable: false),
                    AcilisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KapanisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcilisNakit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_Vardiya", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vardiya_Cihaz_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihaz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vardiya_Kasa_KasaId",
                        column: x => x.KasaId,
                        principalTable: "Kasa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vardiya_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KasaHareket",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KasaId = table.Column<long>(type: "bigint", nullable: false),
                    VardiyaId = table.Column<long>(type: "bigint", nullable: true),
                    KullaniciId = table.Column<long>(type: "bigint", nullable: false),
                    CihazId = table.Column<long>(type: "bigint", nullable: false),
                    IslemTipi = table.Column<int>(type: "int", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferansTip = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferansId = table.Column<long>(type: "bigint", nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_KasaHareket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KasaHareket_Cihaz_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihaz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KasaHareket_Kasa_KasaId",
                        column: x => x.KasaId,
                        principalTable: "Kasa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KasaHareket_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KasaHareket_Vardiya_VardiyaId",
                        column: x => x.VardiyaId,
                        principalTable: "Vardiya",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VardiyaKapanis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VardiyaId = table.Column<long>(type: "bigint", nullable: false),
                    BeklenenNakit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SayilanNakit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FarkTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KartToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VeresiyeToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_VardiyaKapanis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VardiyaKapanis_Vardiya_VardiyaId",
                        column: x => x.VardiyaId,
                        principalTable: "Vardiya",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Cihaz",
                keyColumn: "Id",
                keyValue: 1L,
                column: "VarsayilanKasaId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Cihaz_VarsayilanKasaId",
                table: "Cihaz",
                column: "VarsayilanKasaId");

            migrationBuilder.CreateIndex(
                name: "IX_Kasa_TenantId_SubeId_Ad",
                table: "Kasa",
                columns: new[] { "TenantId", "SubeId", "Ad" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareket_CihazId",
                table: "KasaHareket",
                column: "CihazId");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareket_KasaId_Tarih",
                table: "KasaHareket",
                columns: new[] { "KasaId", "Tarih" });

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareket_KullaniciId",
                table: "KasaHareket",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareket_VardiyaId",
                table: "KasaHareket",
                column: "VardiyaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vardiya_CihazId_AktifMi",
                table: "Vardiya",
                columns: new[] { "CihazId", "AktifMi" });

            migrationBuilder.CreateIndex(
                name: "IX_Vardiya_KasaId_AktifMi",
                table: "Vardiya",
                columns: new[] { "KasaId", "AktifMi" });

            migrationBuilder.CreateIndex(
                name: "IX_Vardiya_KullaniciId",
                table: "Vardiya",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_VardiyaKapanis_VardiyaId",
                table: "VardiyaKapanis",
                column: "VardiyaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cihaz_Kasa_VarsayilanKasaId",
                table: "Cihaz",
                column: "VarsayilanKasaId",
                principalTable: "Kasa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cihaz_Kasa_VarsayilanKasaId",
                table: "Cihaz");

            migrationBuilder.DropTable(
                name: "KasaHareket");

            migrationBuilder.DropTable(
                name: "VardiyaKapanis");

            migrationBuilder.DropTable(
                name: "Vardiya");

            migrationBuilder.DropTable(
                name: "Kasa");

            migrationBuilder.DropIndex(
                name: "IX_Cihaz_VarsayilanKasaId",
                table: "Cihaz");

            migrationBuilder.DropColumn(
                name: "VarsayilanKasaId",
                table: "Cihaz");
        }
    }
}
