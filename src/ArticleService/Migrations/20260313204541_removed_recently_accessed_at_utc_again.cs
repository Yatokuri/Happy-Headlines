using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticleService.Migrations
{
    /// <inheritdoc />
    public partial class removed_recently_accessed_at_utc_again : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecentlyAccessedAtUtc",
                table: "articles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecentlyAccessedAtUtc",
                table: "articles",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
