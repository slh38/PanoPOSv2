using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSystemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanici",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_Kullanici", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
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
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                    table.UniqueConstraint("AK_Tenant_TenantId", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciRol",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<long>(type: "bigint", nullable: false),
                    RolId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_KullaniciRol", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KullaniciRol_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KullaniciRol_Rol_RolId",
                        column: x => x.RolId,
                        principalTable: "Rol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sube",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_Sube", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sube_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cihaz",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_Cihaz", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cihaz_Sube_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Sube",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciSube",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<long>(type: "bigint", nullable: false),
                    BagliSubeId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_KullaniciSube", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KullaniciSube_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KullaniciSube_Sube_BagliSubeId",
                        column: x => x.BagliSubeId,
                        principalTable: "Sube",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciOturum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<long>(type: "bigint", nullable: false),
                    CihazId = table.Column<long>(type: "bigint", nullable: false),
                    GirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CikisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_KullaniciOturum", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KullaniciOturum_Cihaz_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihaz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KullaniciOturum_Kullanici_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Kullanici",
                columns: new[] { "Id", "Ad", "AktifMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "OlusturanKullaniciId", "OlusturmaTarihi", "Pin", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "Soyad", "SubeId", "TenantId" },
                values: new object[] { 1L, "Admin", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), "1234", null, false, null, "Kullanici", 1L, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "Rol",
                columns: new[] { "Id", "Ad", "AktifMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "Kod", "OlusturanKullaniciId", "OlusturmaTarihi", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[] { 1L, "Admin", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "ADMIN", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "Tenant",
                columns: new[] { "Id", "Ad", "AktifMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "Kod", "OlusturanKullaniciId", "OlusturmaTarihi", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[] { 1L, "Pano Demo Tenant", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "PANO", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "KullaniciRol",
                columns: new[] { "Id", "AktifMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "KullaniciId", "OlusturanKullaniciId", "OlusturmaTarihi", "RolId", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[] { 1L, true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, 1L, null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1L, null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "Sube",
                columns: new[] { "Id", "Ad", "AktifMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "Kod", "OlusturanKullaniciId", "OlusturmaTarihi", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[] { 1L, "Merkez Sube", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "MRKZ", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "Cihaz",
                columns: new[] { "Id", "Ad", "AktifMi", "GuncellemeTarihi", "GuncelleyenKullaniciId", "Kod", "OlusturanKullaniciId", "OlusturmaTarihi", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[] { 1L, "Ana Kasa Cihaz", true, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "CIHAZ-001", null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "KullaniciSube",
                columns: new[] { "Id", "AktifMi", "BagliSubeId", "GuncellemeTarihi", "GuncelleyenKullaniciId", "KullaniciId", "OlusturanKullaniciId", "OlusturmaTarihi", "SilenKullaniciId", "SilindiMi", "SilinmeTarihi", "SubeId", "TenantId" },
                values: new object[] { 1L, true, 1L, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, 1L, null, new DateTime(2026, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, 1L, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.CreateIndex(
                name: "IX_Cihaz_SubeId",
                table: "Cihaz",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cihaz_TenantId_Kod",
                table: "Cihaz",
                columns: new[] { "TenantId", "Kod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanici_TenantId_Pin",
                table: "Kullanici",
                columns: new[] { "TenantId", "Pin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciOturum_CihazId",
                table: "KullaniciOturum",
                column: "CihazId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciOturum_KullaniciId",
                table: "KullaniciOturum",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciRol_KullaniciId_RolId",
                table: "KullaniciRol",
                columns: new[] { "KullaniciId", "RolId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciRol_RolId",
                table: "KullaniciRol",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciSube_BagliSubeId",
                table: "KullaniciSube",
                column: "BagliSubeId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciSube_KullaniciId_BagliSubeId",
                table: "KullaniciSube",
                columns: new[] { "KullaniciId", "BagliSubeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rol_TenantId_Kod",
                table: "Rol",
                columns: new[] { "TenantId", "Kod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sube_TenantId_Kod",
                table: "Sube",
                columns: new[] { "TenantId", "Kod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_Kod",
                table: "Tenant",
                column: "Kod",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KullaniciOturum");

            migrationBuilder.DropTable(
                name: "KullaniciRol");

            migrationBuilder.DropTable(
                name: "KullaniciSube");

            migrationBuilder.DropTable(
                name: "Cihaz");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "Kullanici");

            migrationBuilder.DropTable(
                name: "Sube");

            migrationBuilder.DropTable(
                name: "Tenant");
        }
    }
}
