using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VidSharePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddThumpnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "Videos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "Videos");
        }
    }
}
