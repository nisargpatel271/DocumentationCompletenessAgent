using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentationCompleteness.Api.Models
{
    [Table("analysis_jobs")]
    public class AnalysisJob
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
        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "Queued"; // Queued, Running, Completed, Failed

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("log")]
        public string? Log { get; set; } // Simple log of steps taken

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
