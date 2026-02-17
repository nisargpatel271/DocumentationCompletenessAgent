using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DocumentationCompleteness.Api.Models
{
    [Table("documentation_gaps")]
    public class DocumentationGap
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("repository_id")]
        public Guid RepositoryId { get; set; }

        [ForeignKey("RepositoryId")]
        public Repository Repository { get; set; } = null!;

        [Required]
        [Column("job_id")]
        public Guid JobId { get; set; }

        [ForeignKey("JobId")]
        public AnalysisJob Job { get; set; } = null!;

        [Required]
        [Column("result_id")]
        public Guid ResultId { get; set; }

        [ForeignKey("ResultId")]
        public AnalysisResult AnalysisResult { get; set; } = null!;

        [Required]
        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [Column("line_number")]
        public int LineNumber { get; set; }

        [Required]
        [Column("element_name")]
        public string ElementName { get; set; } = string.Empty; // e.g. "CreateUser"

        [Required]
        [Column("element_type")]
        public string ElementType { get; set; } = string.Empty; // e.g. "Method", "Class"

        [Required]
        [Column("gap_type")]
        public string GapType { get; set; } = "Missing"; // Missing, Incomplete

        [Required]
        [Column("severity")]
        public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low

        [Column("message")]
        public string? Message { get; set; }

        [Column("missing_coverage_type")]
        public string? MissingCoverageType { get; set; } // e.g. "Summary", "Returns", "Params"

        [Column("status")]
        public string Status { get; set; } = "Open"; // Open, Resolved
    }
}
