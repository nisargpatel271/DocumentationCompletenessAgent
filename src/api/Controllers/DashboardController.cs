using System.Threading.Tasks;
using DocumentationCompleteness.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentationCompleteness.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _dashboardService.GetSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("trends")]
        public async Task<IActionResult> GetTrends([FromQuery] int days = 30)
        {
            var trends = await _dashboardService.GetTrendsAsync(days);
            return Ok(trends);
        }
    }
}
