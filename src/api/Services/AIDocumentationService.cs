using System;
using System.Threading;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Data;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Services.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocumentationCompleteness.Api.Services
{
    public class AIDocumentationService
    {
        private readonly ApplicationDbContext _context;
        private readonly AzureOpenAIClientWrapper _aiClient;
        private readonly PromptTemplateEngine _promptEngine;
        private readonly ResponseValidator _validator;
        private readonly ILogger<AIDocumentationService> _logger;

        public AIDocumentationService(
            ApplicationDbContext context,
            AzureOpenAIClientWrapper aiClient,
            PromptTemplateEngine promptEngine,
            ResponseValidator validator,
            ILogger<AIDocumentationService> logger)
        {
            _context = context;
            _aiClient = aiClient;
            _promptEngine = promptEngine;
            _validator = validator;
            _logger = logger;
        }

        public async Task<AISuggestion> GenerateSuggestionAsync(
            DocumentationGap gap,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Requesting AI documentation for gap {GapId}...", gap.Id);

                var prompt = _promptEngine.BuildPrompt(gap);
                var systemPrompt = _promptEngine.GetSystemPrompt();
                
                var generatedDocs = await _aiClient.GenerateAsync(prompt, systemPrompt, ct);

                var validation = _validator.Validate(generatedDocs, gap.Language);
                if (!validation.IsValid)
                {
                    throw new Exception("AI generated an invalid or empty response.");
                }

                var suggestion = new AISuggestion
                {
                    Id = Guid.NewGuid(),
                    GapId = gap.Id,
                    ElementName = gap.ElementName,
                    ElementType = gap.ElementType,
                    Language = gap.Language,
                    GeneratedDocumentation = generatedDocs,
                    ConfidenceScore = validation.Score,
                    NeedsHumanReview = validation.NeedsHumanReview,
                    Status = "Pending",
                    GeneratedAt = DateTime.UtcNow
                };

                _context.AISuggestions.Add(suggestion);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Successfully generated AISuggestion {SuggestionId} for gap {GapId}", suggestion.Id, gap.Id);

                return suggestion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI documentation for gap {GapId}", gap.Id);
                throw;
            }
        }

        public async Task ProcessGapsBatchAsync(System.Collections.Generic.List<DocumentationGap> gaps, CancellationToken ct = default)
        {
            foreach (var gap in gaps)
            {
                if (ct.IsCancellationRequested) break;
                try
                {
                    await GenerateSuggestionAsync(gap, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate bulk suggestion for gap {GapId}", gap.Id);
                }
            }
        }

    }
}
