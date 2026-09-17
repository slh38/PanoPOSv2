using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanoPos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOpaqueSessionContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OturumTokenHash",
                table: "KullaniciOturum",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciOturum_OturumTokenHash",
                table: "KullaniciOturum",
                column: "OturumTokenHash",
                unique: true,
                filter: "[OturumTokenHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KullaniciOturum_OturumTokenHash",
                table: "KullaniciOturum");

            migrationBuilder.DropColumn(
                name: "OturumTokenHash",
                table: "KullaniciOturum");
        }
    }
}
