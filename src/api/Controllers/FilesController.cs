using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace DocumentationCompleteness.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpGet("content")]
        public async Task<IActionResult> GetFileContent([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is required.");
            }

            if (!System.IO.File.Exists(path))
            {
                return NotFound($"File not found: {path}");
            }

            try
            {
                var content = await System.IO.File.ReadAllTextAsync(path);
                return Ok(new { content });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error reading file: {ex.Message}");
            }
        }
    }
}
