using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VidSharePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoragePathToVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "Videos");
        }
    }
}
