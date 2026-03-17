using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentationCompleteness.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageAndSnippetToGap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code_snippet",
                table: "documentation_gaps",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "documentation_gaps",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code_snippet",
                table: "documentation_gaps");

            migrationBuilder.DropColumn(
                name: "language",
                table: "documentation_gaps");
        }
    }
}
