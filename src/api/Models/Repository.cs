using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentationCompleteness.Api.Models
{
    [Table("repositories")]
    public class Repository
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("source")]
        [MaxLength(50)]
        public string Source { get; set; } = string.Empty;

        [Required]
        [Column("repository_url")]
        public string RepositoryUrl { get; set; } = string.Empty;

        [Column("default_branch")]
        [MaxLength(100)]
        public string DefaultBranch { get; set; } = "main";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("last_scanned_at")]
        public DateTime? LastScannedAt { get; set; }

        [Column("scan_frequency")]
        [MaxLength(50)]
        public string? ScanFrequency { get; set; }

        [Column("settings", TypeName = "jsonb")]
        public string? Settings { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        [MaxLength(255)]
        public string? CreatedBy { get; set; }
    }
}
