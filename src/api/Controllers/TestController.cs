using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentationCompleteness.Api.Data;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Services;

namespace DocumentationCompleteness.Api.Controllers
{
    [Route("api/test")]
    [ApiController]

    // TEMPORARY CONTROLLER: To be deleted after verification
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IGitService _gitService;
        private readonly ILogger<TestController> _logger;

        public TestController(ApplicationDbContext context, IGitService gitService, ILogger<TestController> logger)
        {
            _context = context;
            _gitService = gitService;
            _logger = logger;
        }

        // 1. Verify Database: Create a dummy AnalysisJob
        [HttpPost("db-check")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var repo = await _context.Repositories.FirstOrDefaultAsync();
                if (repo == null)
                    return BadRequest("No repositories found in DB. Add one first!");

                var job = new AnalysisJob
                {
                    Id = Guid.NewGuid(),
                    RepositoryId = repo.Id,
                    Status = "Test_Created",
                    StartedAt = DateTime.UtcNow
                };

                _context.AnalysisJobs.Add(job);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Successfully wrote to DB!", JobId = job.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Stack = ex.StackTrace });
            }
        }

        // 2. Verify Git Service: Clone a small public repo
        [HttpPost("git-clone")]
        public async Task<IActionResult> TestGitClone([FromQuery] string url = "https://github.com/octocat/Hello-World.git")
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), "DocAgent_Test", Guid.NewGuid().ToString().Substring(0, 8));
                
                _logger.LogInformation("Testing clone to {Path}", tempPath);
                
                await _gitService.CloneRepositoryAsync(url, tempPath);

                var files = Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories);
                var fileNames = files.Select(Path.GetFileName).Take(5).ToArray();
                
                // Cleanup immediately
                try { Directory.Delete(tempPath, true); } catch { /* ignore */ }

                return Ok(new { 
                    Message = "Successfully cloned!", 
                    Path = tempPath, 
                    FileCount = files.Length,
                    Files = fileNames
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
