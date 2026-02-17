using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task<AnalysisJob> RunAnalysisAsync(Guid repositoryId)
        {
            // 1. Validate Repository
            var repo = await _context.Repositories.FindAsync(repositoryId);
            if (repo == null) throw new ArgumentException("Repository not found", nameof(repositoryId));

            // 2. Create Job
            var job = new AnalysisJob
            {
                Id = Guid.NewGuid(),
                RepositoryId = repositoryId,
                Status = "Running",
                StartedAt = DateTime.UtcNow,
                Log = "Analysis started..."
            };
            
            _context.AnalysisJobs.Add(job);
            await _context.SaveChangesAsync();

            string tempPath = Path.Combine(Path.GetTempPath(), "DocAgent", Guid.NewGuid().ToString());

            try
            {
                // 3. Clone Repository
                _logger.LogInformation("Cloning repository {RepoName} to {Path}...", repo.Name, tempPath);
                await _gitService.CloneRepositoryAsync(repo.RepositoryUrl, tempPath);

                // 4. Scan Files
                var files = _fileService.GetSourceFiles(tempPath);
                _logger.LogInformation("Found {Count} source files.", files.Count());

                var analysisResult = new AnalysisResult
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    RepositoryId = repositoryId, // Add to match schema requirements if present
                    TotalFiles = files.Count(),
                    AnalyzedFiles = 0,
                    CreatedAt = DateTime.UtcNow
                };

                var allGaps = new List<DocumentationGap>();
                int totalElements = 0;
                int documentedElements = 0;
                long totalWeightedScore = 0;
                long actualWeightedScore = 0;

                // 5. Run Analyzers
                foreach (var file in files)
                {
                    var analyzer = _analyzers.FirstOrDefault(a => a.SupportsFile(file));
                    if (analyzer != null)
                    {
                        try 
                        {
                            var fileResult = await analyzer.AnalyzeFileAsync(file, repositoryId, job.Id);
                            
                            analysisResult.AnalyzedFiles++;
                            
                            // Metrics
                            totalElements += fileResult.TotalElements;
                            documentedElements += fileResult.DocumentedElements;
                            totalWeightedScore += fileResult.TotalWeightedScore;
                            actualWeightedScore += fileResult.ActualWeightedScore;

                            // Collect Gaps
                            foreach (var gap in fileResult.Gaps)
                            {
                                gap.ResultId = analysisResult.Id; // Link to Result
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

                // 6. Calculate Final Scores
                analysisResult.TotalGaps = allGaps.Count;
                analysisResult.CriticalGaps = allGaps.Count(g => g.Severity == "Critical");
                analysisResult.HighPriorityGaps = allGaps.Count(g => g.Severity == "High");
                analysisResult.MediumPriorityGaps = allGaps.Count(g => g.Severity == "Medium");
                analysisResult.LowPriorityGaps = allGaps.Count(g => g.Severity == "Low");

                // Calculate Weighted Coverage %
                if (totalWeightedScore > 0)
                {
                    analysisResult.OverallCoverage = Math.Round((double)actualWeightedScore / totalWeightedScore * 100, 2);
                }
                else
                {
                    analysisResult.OverallCoverage = 100; // Default if no elements found (e.g. empty files)
                }

                // 7. Save Results
                _context.AnalysisResults.Add(analysisResult);
                
                // Batch add gaps for performance
                // If gaps list is huge (>1000), we might want to batch add. For now, AddRange is fine.
                _context.DocumentationGaps.AddRange(allGaps);

                // Update Job Status
                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;
                job.Log += $"\nCompleted successfully. Analyzed {analysisResult.AnalyzedFiles}/{analysisResult.TotalFiles} files. Score: {analysisResult.OverallCoverage}%";

                // Update Repository Last Scanned
                repo.LastScannedAt = DateTime.UtcNow;

                // 8. Commit
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Analysis complete. Job {JobId}. Score: {Score}%", job.Id, analysisResult.OverallCoverage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis failed for Job {JobId}", job.Id);
                job.Status = "Failed";
                job.Log += $"\nFailed: {ex.Message}";
                job.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                throw;
            }
            finally
            {
                // 9. Cleanup
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

            return job;
        }
    }
}
