using System;
using System.Threading;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Data;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocumentationCompleteness.Api.Controllers
{
    [ApiController]
    [Route("api/documentation")]
    public class DocumentationController : ControllerBase
    {
        private readonly AIDocumentationService _aiService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentationController> _logger;

        public DocumentationController(
            AIDocumentationService aiService,
            ApplicationDbContext context,
            ILogger<DocumentationController> logger)
        {
            _aiService = aiService;
            _context = context;
            _logger = logger;
        }

        // POST /api/documentation/generate
        [HttpPost("generate")]
        public async Task<IActionResult> Generate(
            [FromBody] GenerateRequest request,
            CancellationToken ct)
        {
            var gap = await _context.DocumentationGaps
                .FirstOrDefaultAsync(g => g.Id == request.GapId, ct);

            if (gap == null)
                return NotFound($"Gap {request.GapId} not found");

            if (string.IsNullOrEmpty(gap.CodeSnippet))
                return BadRequest("Gap has no code snippet — re-run analysis first");

            if (string.IsNullOrEmpty(gap.Language) ||
                gap.Language is "cpp" or "unknown")
                return BadRequest($"AI generation not supported for language: {gap.Language}");

            var existing = await _context.AISuggestions
                .FirstOrDefaultAsync(s => s.GapId == request.GapId, ct);

            if (existing != null)
                return Ok(existing);

            var suggestion = await _aiService.GenerateSuggestionAsync(gap, ct);
            return Ok(suggestion);
        }

        // POST /api/documentation/suggestions/{id}/accept
        [HttpPost("suggestions/{id}/accept")]
        public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
        {
            var suggestion = await _context.AISuggestions
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (suggestion == null)
                return NotFound();

            suggestion.Status = "Accepted";
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Accepted AISuggestion {SuggestionId}", id);

            return Ok();
        }

        // POST /api/documentation/suggestions/{id}/reject
        [HttpPost("suggestions/{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
        {
            var suggestion = await _context.AISuggestions
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (suggestion == null)
                return NotFound();

            suggestion.Status = "Rejected";
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Rejected AISuggestion {SuggestionId}", id);

            return Ok();
        }

        // GET /api/documentation/gaps/{id}
        [HttpGet("gaps/{id}")]
        public async Task<IActionResult> GetGap(Guid id, CancellationToken ct)
        {
            var gap = await _context.DocumentationGaps
                .FirstOrDefaultAsync(g => g.Id == id, ct);

            if (gap == null)
                return NotFound();

            return Ok(gap);
        }
    }

    public record GenerateRequest(Guid GapId);
}
