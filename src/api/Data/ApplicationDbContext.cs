using Microsoft.EntityFrameworkCore;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Repository> Repositories { get; set; }
        public DbSet<AnalysisJob> AnalysisJobs { get; set; }
        public DbSet<AnalysisResult> AnalysisResults { get; set; }
        public DbSet<DocumentationGap> DocumentationGaps { get; set; }
        public DbSet<AISuggestion> AISuggestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
