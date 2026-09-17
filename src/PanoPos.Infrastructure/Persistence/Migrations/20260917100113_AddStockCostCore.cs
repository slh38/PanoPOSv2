using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockCostCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaliyetYontemi",
                table: "TenantAyar",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "BirimMaliyet",
                table: "FaturaDetay",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaliyetYontemi",
                table: "FaturaDetay",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateTable(
                name: "StokMaliyet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubeId = table.Column<long>(type: "bigint", nullable: false),
                    DepoId = table.Column<long>(type: "bigint", nullable: false),
                    StokKartId = table.Column<long>(type: "bigint", nullable: false),
                    StokKartVaryantId = table.Column<long>(type: "bigint", nullable: true),
                    SonAlisMaliyeti = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    AgirlikliOrtalamaMaliyet = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SonAlisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SonGuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokMaliyet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StokMaliyet_Depo_DepoId",
                        column: x => x.DepoId,
                        principalTable: "Depo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokMaliyet_StokKartVaryant_StokKartVaryantId",
                        column: x => x.StokKartVaryantId,
                        principalTable: "StokKartVaryant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokMaliyet_StokKart_StokKartId",
                        column: x => x.StokKartId,
                        principalTable: "StokKart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokMaliyet_Sube_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Sube",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StokMaliyet_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "TenantAyar",
                keyColumn: "Id",
                keyValue: 1L,
                column: "MaliyetYontemi",
                value: 2);

            migrationBuilder.CreateIndex(
                name: "IX_StokMaliyet_DepoId",
                table: "StokMaliyet",
                column: "DepoId");

            migrationBuilder.CreateIndex(
                name: "IX_StokMaliyet_StokKartId",
                table: "StokMaliyet",
                column: "StokKartId");

            migrationBuilder.CreateIndex(
                name: "IX_StokMaliyet_StokKartVaryantId",
                table: "StokMaliyet",
                column: "StokKartVaryantId");

            migrationBuilder.CreateIndex(
                name: "IX_StokMaliyet_SubeId",
                table: "StokMaliyet",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_StokMaliyet_TenantId_SubeId_DepoId_StokKartId",
                table: "StokMaliyet",
                columns: new[] { "TenantId", "SubeId", "DepoId", "StokKartId" },
                unique: true,
                filter: "[StokKartVaryantId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StokMaliyet_TenantId_SubeId_DepoId_StokKartId_StokKartVaryantId",
                table: "StokMaliyet",
                columns: new[] { "TenantId", "SubeId", "DepoId", "StokKartId", "StokKartVaryantId" },
                unique: true,
                filter: "[StokKartVaryantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StokMaliyet");

            migrationBuilder.DropColumn(
                name: "MaliyetYontemi",
                table: "TenantAyar");

            migrationBuilder.DropColumn(
                name: "BirimMaliyet",
                table: "FaturaDetay");

            migrationBuilder.DropColumn(
                name: "MaliyetYontemi",
                table: "FaturaDetay");
        }
    }
}
