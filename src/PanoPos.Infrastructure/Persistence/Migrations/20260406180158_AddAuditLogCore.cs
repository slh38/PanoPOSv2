using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IslemLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    CihazId = table.Column<long>(type: "bigint", nullable: true),
                    KullaniciId = table.Column<long>(type: "bigint", nullable: true),
                    ModulAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EkranAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ButonAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IslemTipi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HedefTablo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HedefId = table.Column<long>(type: "bigint", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BasariliMi = table.Column<bool>(type: "bit", nullable: false),
                    HataKodu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HataMesaji = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SureMs = table.Column<long>(type: "bigint", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IslemLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IslemLog_Cihaz_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihaz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IslemLog_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IslemLog_CihazId",
                table: "IslemLog",
                column: "CihazId");

            migrationBuilder.CreateIndex(
                name: "IX_IslemLog_KullaniciId",
                table: "IslemLog",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_IslemLog_TenantId_BasariliMi_OlusturmaTarihi",
                table: "IslemLog",
                columns: new[] { "TenantId", "BasariliMi", "OlusturmaTarihi" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_IslemLog_TenantId_KullaniciId_OlusturmaTarihi",
                table: "IslemLog",
                columns: new[] { "TenantId", "KullaniciId", "OlusturmaTarihi" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_IslemLog_TenantId_SubeId_OlusturmaTarihi",
                table: "IslemLog",
                columns: new[] { "TenantId", "SubeId", "OlusturmaTarihi" },
                descending: new[] { false, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IslemLog");
        }
    }
}
