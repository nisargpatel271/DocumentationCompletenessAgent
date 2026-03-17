using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services.AI
{
    public interface IPromptService
    {
        string GenerateDocumentationPrompt(DocumentationGap gap);
    }
}
