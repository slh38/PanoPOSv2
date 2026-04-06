using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxOlay",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    CihazId = table.Column<long>(type: "bigint", nullable: false),
                    OlayTipi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KaynakTablo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KaynakId = table.Column<long>(type: "bigint", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Durum = table.Column<short>(type: "smallint", nullable: false),
                    DenemeSayisi = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GonderimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SonHataMesaji = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxOlay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxOlay_Cihaz_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihaz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxOlay_CihazId",
                table: "OutboxOlay",
                column: "CihazId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxOlay_KaynakTablo_KaynakId",
                table: "OutboxOlay",
                columns: new[] { "KaynakTablo", "KaynakId" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxOlay_TenantId_SubeId_Durum_OlusturmaTarihi",
                table: "OutboxOlay",
                columns: new[] { "TenantId", "SubeId", "Durum", "OlusturmaTarihi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxOlay");
        }
    }
}
