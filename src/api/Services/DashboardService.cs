using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Data;
using DocumentationCompleteness.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentationCompleteness.Api.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetSummaryAsync()
        {
            var repos = await _context.Repositories.ToListAsync();
            var results = await _context.AnalysisResults
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Get the latest result for each repository to calculate overall health
            var latestResultsPerRepo = results
                .GroupBy(r => r.RepositoryId)
                .Select(g => g.First())
                .ToList();

            var totalGaps = latestResultsPerRepo.Sum(r => r.TotalGaps);
            var criticalGaps = latestResultsPerRepo.Sum(r => r.CriticalGaps);
            
            double avgCoverage = latestResultsPerRepo.Any() 
                ? latestResultsPerRepo.Average(r => (double)r.OverallCoverage) 
                : 0;

            var topGapsRepos = latestResultsPerRepo
                .OrderByDescending(r => r.CriticalGaps)
                .Take(5)
                .Select(r => new {
                    Id = r.RepositoryId,
                    Name = repos.FirstOrDefault(repo => repo.Id == r.RepositoryId)?.Name ?? "Unknown",
                    Coverage = r.OverallCoverage,
                    CriticalGaps = r.CriticalGaps,
                    TotalGaps = r.TotalGaps
                });

            var recentJobs = await _context.AnalysisJobs
                .Include(j => j.Repository)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            return new
            {
                OverallCoverage = Math.Round(avgCoverage, 1),
                TotalGaps = totalGaps,
                CriticalGaps = criticalGaps,
                TotalRepositories = repos.Count,
                TopGapsRepos = topGapsRepos,
                RecentJobs = recentJobs.Select(j => new {
                    j.Id,
                    RepoName = j.Repository?.Name,
                    j.Status,
                    j.CreatedAt,
                    j.CompletedAt
                })
            };
        }
        public async Task<object> GetTrendsAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            var results = await _context.AnalysisResults
                .Include(r => r.Repository)
                .Where(r => r.CreatedAt >= startDate)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            var overall = results
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Coverage = Math.Round(g.Average(r => r.OverallCoverage), 2)
                })
                .ToList();

            var byRepository = results
                .GroupBy(r => new { r.RepositoryId, r.Repository.Name })
                .Select(group => new
                {
                    RepositoryId = group.Key.RepositoryId,
                    RepositoryName = group.Key.Name,
                    Data = group
                        .GroupBy(r => r.CreatedAt.Date)
                        .Select(g => new
                        {
                            Date = g.Key.ToString("yyyy-MM-dd"),
                            Coverage = Math.Round(g.Average(r => r.OverallCoverage), 2)
                        })
                        .OrderBy(d => d.Date)
                        .ToList()
                })
                .ToList();

            return new
            {
                Overall = overall,
                ByRepository = byRepository
            };
        }
    }
}
