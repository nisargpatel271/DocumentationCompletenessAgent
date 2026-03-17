using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DocumentationCompleteness.Api.Data;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Services.Analysis;

namespace DocumentationCompleteness.Api.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGitService _gitService;
        private readonly IFileService _fileService;
        private readonly IEnumerable<ICodeAnalyzer> _analyzers;
        private readonly ILogger<AnalysisService> _logger;

        public AnalysisService(
            ApplicationDbContext context, 
            IGitService gitService, 
            IFileService fileService, 
            IEnumerable<ICodeAnalyzer> analyzers, 
            ILogger<AnalysisService> logger)
        {
            _context = context;
            _gitService = gitService;
            _fileService = fileService;
            _analyzers = analyzers;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new analysis job in "Queued" status. Returns immediately.
        /// Called by the controller — does NOT run the analysis.
        /// </summary>
        public async Task<AnalysisJob> CreateJobAsync(Guid repositoryId)
        {
            var repo = await _context.Repositories.FindAsync(repositoryId);
            if (repo == null) throw new ArgumentException("Repository not found", nameof(repositoryId));

            var job = new AnalysisJob
            {
                Id = Guid.NewGuid(),
                RepositoryId = repositoryId,
                Status = "Queued",
                CreatedAt = DateTime.UtcNow,
                Log = "Job queued, waiting for worker..."
            };

            _context.AnalysisJobs.Add(job);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created analysis job {JobId} for repository {RepoName}", job.Id, repo.Name);
            return job;
        }

        /// <summary>
        /// Executes the full analysis pipeline for a given job.
        /// Called by the AnalysisWorker background service.
        /// Flow: Clone → Scan → Analyze → Save Gaps → Cleanup
        /// </summary>
        public async Task ExecuteJobAsync(Guid jobId, CancellationToken ct = default)
        {
            var job = await _context.AnalysisJobs
                .Include(j => j.Repository)
                .FirstOrDefaultAsync(j => j.Id == jobId, ct);

            if (job == null) throw new ArgumentException($"Job {jobId} not found", nameof(jobId));

            var repo = job.Repository;

            // Mark as Running
            job.Status = "Running";
            job.StartedAt = DateTime.UtcNow;
            job.Log += "\nAnalysis started...";
            await _context.SaveChangesAsync(ct);

            string tempPath = Path.Combine(Path.GetTempPath(), "DocAgent", Guid.NewGuid().ToString());

            try
            {
                // 1. Clone Repository
                _logger.LogInformation("Cloning repository {RepoName} to {Path}...", repo.Name, tempPath);
                await _gitService.CloneRepositoryAsync(repo.RepositoryUrl, tempPath);

                // 2. Scan Files
                var files = _fileService.GetSourceFiles(tempPath);
                _logger.LogInformation("Found {Count} source files.", files.Count());

                var analysisResult = new AnalysisResult
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    RepositoryId = repo.Id,
                    TotalFiles = files.Count(),
                    AnalyzedFiles = 0,
                    CreatedAt = DateTime.UtcNow
                };

                var allGaps = new List<DocumentationGap>();
                int totalElements = 0;
                int documentedElements = 0;
                long totalWeightedScore = 0;
                long actualWeightedScore = 0;

                // 3. Run Analyzers
                foreach (var file in files)
                {
                    ct.ThrowIfCancellationRequested();

                    var analyzer = _analyzers.FirstOrDefault(a => a.SupportsFile(file));
                    if (analyzer != null)
                    {
                        try 
                        {
                            var fileResult = await analyzer.AnalyzeFileAsync(file, repo.Id, job.Id);
                            
                            analysisResult.AnalyzedFiles++;
                            
                            // Metrics
                            totalElements += fileResult.TotalElements;
                            documentedElements += fileResult.DocumentedElements;
                            totalWeightedScore += fileResult.TotalWeightedScore;
                            actualWeightedScore += fileResult.ActualWeightedScore;

                            // Collect Gaps
                            foreach (var gap in fileResult.Gaps)
                            {
                                gap.ResultId = analysisResult.Id;
                                allGaps.Add(gap);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error analyzing file {File}", file);
                            // Continue to next file
                        }
                    }
                }

                // 4. Calculate Final Scores
                analysisResult.TotalGaps = allGaps.Count;
                analysisResult.CriticalGaps = allGaps.Count(g => g.Severity == "Critical");
                analysisResult.HighPriorityGaps = allGaps.Count(g => g.Severity == "High");
                analysisResult.MediumPriorityGaps = allGaps.Count(g => g.Severity == "Medium");
                analysisResult.LowPriorityGaps = allGaps.Count(g => g.Severity == "Low");

                if (totalWeightedScore > 0)
                {
                    analysisResult.OverallCoverage = Math.Round((double)actualWeightedScore / totalWeightedScore * 100, 2);
                }
                else
                {
                    analysisResult.OverallCoverage = 100;
                }

                // 5. Save Results
                _context.AnalysisResults.Add(analysisResult);
                _context.DocumentationGaps.AddRange(allGaps);

                // 6. Update Job Status
                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;
                job.Log += $"\nCompleted successfully. Analyzed {analysisResult.AnalyzedFiles}/{analysisResult.TotalFiles} files. Score: {analysisResult.OverallCoverage}%";

                // Update Repository Last Scanned
                repo.LastScannedAt = DateTime.UtcNow;

                // 7. Commit
                await _context.SaveChangesAsync(ct);
                
                _logger.LogInformation("Analysis complete. Job {JobId}. Score: {Score}%", job.Id, analysisResult.OverallCoverage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis failed for Job {JobId}", job.Id);
                job.Status = "Failed";
                job.Log += $"\nFailed: {ex.Message}";
                job.ErrorMessage = ex.Message;
                job.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                throw;
            }
            finally
            {
                // 8. Cleanup
                try
                {
                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cleanup temp path {Path}", tempPath);
                }
            }
        }

        public async Task<List<DocumentationGap>> GetGapsByJobIdAsync(
            Guid jobId,
            CancellationToken ct = default)
        {
            return await _context.DocumentationGaps
                .Where(g => g.JobId == jobId && g.Status == "Open")
                .ToListAsync(ct);
        }
    }
}
