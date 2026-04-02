using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPinLoginCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KullaniciOturum_KullaniciId",
                table: "KullaniciOturum");

            migrationBuilder.DropIndex(
                name: "IX_Kullanici_TenantId_Pin",
                table: "Kullanici");

            migrationBuilder.DropColumn(
                name: "Pin",
                table: "Kullanici");

            migrationBuilder.AddColumn<int>(
                name: "BasarisizGirisSayisi",
                table: "Kullanici",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "KilitliMi",
                table: "Kullanici",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PinHash",
                table: "Kullanici",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PinSonDegistirmeTarihi",
                table: "Kullanici",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SonGirisTarihi",
                table: "Kullanici",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Kullanici",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "BasarisizGirisSayisi", "KilitliMi", "PinHash", "PinSonDegistirmeTarihi", "SonGirisTarihi" },
                values: new object[] { 0, false, "100000.AQIDBAUGBwgJCgsMDQ4PEA==.zXEe8seaNwtLmNvAYvfpAmiMOk6AXt6Jn4slCkkKXHE=", new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciOturum_KullaniciId_AktifMi",
                table: "KullaniciOturum",
                columns: new[] { "KullaniciId", "AktifMi" });

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_TenantId_AktifMi",
                table: "Kullanici",
                columns: new[] { "TenantId", "AktifMi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KullaniciOturum_KullaniciId_AktifMi",
                table: "KullaniciOturum");

            migrationBuilder.DropIndex(
                name: "IX_Kullanici_TenantId_AktifMi",
                table: "Kullanici");

            migrationBuilder.DropColumn(
                name: "BasarisizGirisSayisi",
                table: "Kullanici");

            migrationBuilder.DropColumn(
                name: "KilitliMi",
                table: "Kullanici");

            migrationBuilder.DropColumn(
                name: "PinHash",
                table: "Kullanici");

            migrationBuilder.DropColumn(
                name: "PinSonDegistirmeTarihi",
                table: "Kullanici");

            migrationBuilder.DropColumn(
                name: "SonGirisTarihi",
                table: "Kullanici");

            migrationBuilder.AddColumn<string>(
                name: "Pin",
                table: "Kullanici",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Kullanici",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Pin",
                value: "1234");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciOturum_KullaniciId",
                table: "KullaniciOturum",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_TenantId_Pin",
                table: "Kullanici",
                columns: new[] { "TenantId", "Pin" },
                unique: true);
        }
    }
}
