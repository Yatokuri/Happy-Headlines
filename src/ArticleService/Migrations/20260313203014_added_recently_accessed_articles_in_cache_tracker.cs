using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticleService.Migrations
{
    /// <inheritdoc />
    public partial class added_recently_accessed_articles_in_cache_tracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecentlyAccessedAtUtc",
                table: "articles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecentlyAccessedAtUtc",
                table: "articles");
        }
    }
}
