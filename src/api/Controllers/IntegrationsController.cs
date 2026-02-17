using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Services;

namespace DocumentationCompleteness.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationsController : ControllerBase
    {
        private readonly IGitHubService _gitHubService;
        private readonly IAzureDevOpsService _adoService;

        public IntegrationsController(IGitHubService gitHubService, IAzureDevOpsService adoService)
        {
            _gitHubService = gitHubService;
            _adoService = adoService;
        }

        [HttpGet("github/repos")]
        public async Task<ActionResult<IEnumerable<Repository>>> GetGitHubRepositories()
        {
            try
            {
                var repos = await _gitHubService.GetRepositoriesAsync();
                return Ok(repos);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error fetching GitHub repositories: {ex.Message}");
            }
        }

        [HttpGet("ado/repos")]
        public async Task<ActionResult<IEnumerable<Repository>>> GetAzureDevOpsRepositories()
        {
            try
            {
                var repos = await _adoService.GetRepositoriesAsync();
                return Ok(repos);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error fetching ADO repositories: {ex.Message}");
            }
        }
    }
}
