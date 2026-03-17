using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentationCompleteness.Api.Models
{
    [Table("ai_suggestions")]
    public class AISuggestion
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("gap_id")]
        public Guid GapId { get; set; }

        [ForeignKey("GapId")]
        public DocumentationGap Gap { get; set; } = null!;

        [Required]
        [Column("element_name")]
        public string ElementName { get; set; } = string.Empty;

        [Required]
        [Column("element_type")]
        public string ElementType { get; set; } = string.Empty;

        [Required]
        [Column("language")]
        public string Language { get; set; } = string.Empty;

        [Required]
        [Column("generated_documentation", TypeName = "text")]
        public string GeneratedDocumentation { get; set; } = string.Empty;

        [Column("confidence_score")]
        public double ConfidenceScore { get; set; }

        [Column("needs_human_review")]
        public bool NeedsHumanReview { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Column("generated_at")]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}

