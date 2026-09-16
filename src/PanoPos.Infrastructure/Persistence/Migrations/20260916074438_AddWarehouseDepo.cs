using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseDepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Depo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepoKodu = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_Depo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Depo_Sube_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Sube",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Depo_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Depo",
                columns: new[] { "Id", "Ad", "AktifMi", "DepoKodu", "GuncellemeTarihi", "GuncelleyenKullaniciId", "OlusturanKullaniciId", "OlusturmaTarihi", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId", "VarsayilanMi" },
                values: new object[] { 1L, "Merkez Depo", true, "MERKEZ", new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111"), true });

            migrationBuilder.CreateIndex(
                name: "IX_Depo_SubeId",
                table: "Depo",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_Depo_TenantId_SubeId",
                table: "Depo",
                columns: new[] { "TenantId", "SubeId" },
                unique: true,
                filter: "[VarsayilanMi] = 1 AND [AktifMi] = 1 AND [SilindiMi] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Depo_TenantId_SubeId_DepoKodu",
                table: "Depo",
                columns: new[] { "TenantId", "SubeId", "DepoKodu" },
                unique: true,
                filter: "[AktifMi] = 1 AND [SilindiMi] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Depo");
        }
    }
}
