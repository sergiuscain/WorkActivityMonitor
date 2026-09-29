using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkActivityMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstSeenAndScreenshotFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstSeenTime",
                table: "Clients",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "PendingScreenshotRequest",
                table: "Clients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstSeenTime",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PendingScreenshotRequest",
                table: "Clients");
        }
    }
}
