using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentationCompleteness.Api.Models
{
    [Table("analysis_results")]
    public class AnalysisResult
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("job_id")]
        public Guid JobId { get; set; }

        [ForeignKey("JobId")]
        public AnalysisJob Job { get; set; } = null!;

        [Column("total_files")]
        public int TotalFiles { get; set; }

        [Column("analyzed_files")]
        public int AnalyzedFiles { get; set; }

        [Column("overall_coverage")]
        public double OverallCoverage { get; set; } // Percentage 0-100

        [Column("total_gaps")]
        public int TotalGaps { get; set; }

        [Column("critical_gaps")]
        public int CriticalGaps { get; set; }

        [Column("high_priority_gaps")]
        public int HighPriorityGaps { get; set; }

        [Column("medium_priority_gaps")]
        public int MediumPriorityGaps { get; set; }

        [Column("low_priority_gaps")]
        public int LowPriorityGaps { get; set; }

        [Required]
        [Column("repository_id")]
        public Guid RepositoryId { get; set; }

        [ForeignKey("RepositoryId")]
        public Repository Repository { get; set; } = null!;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for gaps
        public ICollection<DocumentationGap> Gaps { get; set; } = new List<DocumentationGap>();
    }
}
