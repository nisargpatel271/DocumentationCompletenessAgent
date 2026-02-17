using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentationCompleteness.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing analysis tables to ensure clean schema (Dev only approach)
            migrationBuilder.Sql("DROP TABLE IF EXISTS documentation_gaps CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS analysis_results CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS analysis_jobs CASCADE;");

            // Do NOT create Repositories table (it exists)

            migrationBuilder.CreateTable(
                name: "analysis_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    repository_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    log = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_analysis_jobs_repositories_repository_id",
                        column: x => x.repository_id,
                        principalTable: "repositories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "analysis_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_files = table.Column<int>(type: "integer", nullable: false),
                    analyzed_files = table.Column<int>(type: "integer", nullable: false),
                    overall_coverage = table.Column<double>(type: "double precision", nullable: false),
                    total_gaps = table.Column<int>(type: "integer", nullable: false),
                    critical_gaps = table.Column<int>(type: "integer", nullable: false),
                    high_priority_gaps = table.Column<int>(type: "integer", nullable: false),
                    medium_priority_gaps = table.Column<int>(type: "integer", nullable: false),
                    low_priority_gaps = table.Column<int>(type: "integer", nullable: false),
                    repository_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_analysis_results_analysis_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "analysis_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_analysis_results_repositories_repository_id",
                        column: x => x.repository_id,
                        principalTable: "repositories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "documentation_gaps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    repository_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    element_name = table.Column<string>(type: "text", nullable: false),
                    element_type = table.Column<string>(type: "text", nullable: false),
                    gap_type = table.Column<string>(type: "text", nullable: false),
                    severity = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    missing_coverage_type = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentation_gaps", x => x.id);
                    table.ForeignKey(
                        name: "FK_documentation_gaps_analysis_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "analysis_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_documentation_gaps_analysis_results_result_id",
                        column: x => x.result_id,
                        principalTable: "analysis_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_documentation_gaps_repositories_repository_id",
                        column: x => x.repository_id,
                        principalTable: "repositories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analysis_jobs_repository_id",
                table: "analysis_jobs",
                column: "repository_id");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_results_job_id",
                table: "analysis_results",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_results_repository_id",
                table: "analysis_results",
                column: "repository_id");

            migrationBuilder.CreateIndex(
                name: "IX_documentation_gaps_job_id",
                table: "documentation_gaps",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_documentation_gaps_repository_id",
                table: "documentation_gaps",
                column: "repository_id");

            migrationBuilder.CreateIndex(
                name: "IX_documentation_gaps_result_id",
                table: "documentation_gaps",
                column: "result_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documentation_gaps");

            migrationBuilder.DropTable(
                name: "analysis_results");

            migrationBuilder.DropTable(
                name: "analysis_jobs");

            // Do NOT drop repositories
        }
    }
}
