using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentationCompleteness.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorMessageToAnalysisJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "analysis_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ai_suggestions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gap_id = table.Column<Guid>(type: "uuid", nullable: false),
                    element_name = table.Column<string>(type: "text", nullable: false),
                    element_type = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    generated_documentation = table.Column<string>(type: "text", nullable: false),
                    confidence_score = table.Column<double>(type: "double precision", nullable: false),
                    needs_human_review = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_suggestions", x => x.id);
                    table.ForeignKey(
                        name: "FK_ai_suggestions_documentation_gaps_gap_id",
                        column: x => x.gap_id,
                        principalTable: "documentation_gaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_suggestions_gap_id",
                table: "ai_suggestions",
                column: "gap_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_suggestions");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "analysis_jobs");
        }
    }
}
