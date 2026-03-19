using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentationCompleteness.Api.Data;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Services;
using DocumentationCompleteness.Api.Services.Background;

namespace DocumentationCompleteness.Api.Controllers
{
    [ApiController]
    [Route("api/analysis")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly AnalysisJobQueue _jobQueue;
        private readonly ApplicationDbContext _context;

        public AnalysisController(
            IAnalysisService analysisService,
            AnalysisJobQueue jobQueue,
            ApplicationDbContext context)
        {
            _analysisService = analysisService;
            _jobQueue = jobQueue;
            _context = context;
        }

        // POST: api/analysis/repository/{repoId}
        // Creates a job (status=Queued), enqueues it, returns 202 immediately
        [HttpPost("repository/{repoId}")]
        public async Task<IActionResult> RunAnalysis(Guid repoId)
        {
            try
            {
                var job = await _analysisService.CreateJobAsync(repoId);
                await _jobQueue.EnqueueAsync(job.Id);
                return AcceptedAtAction(nameof(GetJobStatus), new { jobId = job.Id }, job);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Analysis failed to queue", Message = ex.Message });
            }
        }

        // GET: api/analysis/{jobId}
        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJobStatus(Guid jobId)
        {
            var job = await _context.AnalysisJobs.FindAsync(jobId);
            if (job == null) return NotFound("Job not found");
            return Ok(job);
        }

        // GET: api/analysis/{jobId}/status
        [HttpGet("{jobId}/status")]
        public async Task<IActionResult> GetJobStatusSlim(Guid jobId)
        {
            var job = await _context.AnalysisJobs
                .Where(j => j.Id == jobId)
                .Select(j => new
                {
                    j.Id,
                    j.RepositoryId,
                    j.Status,
                    j.CreatedAt,
                    j.StartedAt,
                    j.CompletedAt,
                    ErrorMessage = (j.Status == "Failed") ? (j.ErrorMessage ?? j.Log) : null
                })
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound($"Job {jobId} not found");

            return Ok(job);
        }

        // GET: api/analysis/results/{jobId}
        [HttpGet("results/{jobId}")]
        public async Task<IActionResult> GetJobResults(Guid jobId)
        {
            var result = await _context.AnalysisResults
                .Include(r => r.Gaps)
                .FirstOrDefaultAsync(r => r.JobId == jobId);
            
            if (result == null) return NotFound("Results not found for this job");
            return Ok(result);
        }
    }
}
