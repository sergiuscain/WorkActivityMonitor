using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkActivityMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddScreenshotFlagToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "Screenshots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "Screenshots");
        }
    }
}
