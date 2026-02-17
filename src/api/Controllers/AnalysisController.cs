using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentationCompleteness.Api.Data;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Services;

namespace DocumentationCompleteness.Api.Controllers
{
    [ApiController]
    [Route("api/analysis")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly ApplicationDbContext _context;

        public AnalysisController(IAnalysisService analysisService, ApplicationDbContext context)
        {
            _analysisService = analysisService;
            _context = context;
        }

        // POST: api/analysis/repository/{repoId}
        [HttpPost("repository/{repoId}")]
        public async Task<IActionResult> RunAnalysis(Guid repoId)
        {
            try
            {
                var job = await _analysisService.RunAnalysisAsync(repoId);
                return AcceptedAtAction(nameof(GetJobStatus), new { jobId = job.Id }, job);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Analysis failed", Message = ex.Message });
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
