using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VidSharePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixVideoProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalSizeBytes",
                table: "Videos",
                newName: "FileSizeInBytes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileSizeInBytes",
                table: "Videos",
                newName: "TotalSizeBytes");
        }
    }
}
