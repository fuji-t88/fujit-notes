using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FujitNotes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleCategoryAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "articles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "tags",
                table: "articles",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "articles");
        }
    }
}
