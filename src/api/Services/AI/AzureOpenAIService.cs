using System;
using System.ClientModel;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using DocumentationCompleteness.Api.Models.Configuration;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace DocumentationCompleteness.Api.Services.AI
{
    public class AzureOpenAIService : IAIService
    {
        private readonly ChatClient _chatClient;
        private readonly AzureOpenAISettings _settings;

        public AzureOpenAIService(IOptions<IntegrationSettings> settings)
        {
            _settings = settings.Value.AzureOpenAI;

            if (string.IsNullOrEmpty(_settings.Endpoint) || string.IsNullOrEmpty(_settings.ApiKey))
            {
                throw new InvalidOperationException("Azure OpenAI configuration is missing. Check appsettings.json.");
            }

            var client = new AzureOpenAIClient(
                new Uri(_settings.Endpoint), 
                new ApiKeyCredential(_settings.ApiKey));

            _chatClient = client.GetChatClient(_settings.DeploymentName);
        }

        public async Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default)
        {
            try
            {
                var messages = new ChatMessage[]
                {
                    new SystemChatMessage("You are a documentation assistant for software engineers."),
                    new UserChatMessage(prompt)
                };

                ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: ct);
                
                return completion.Content[0].Text?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                // In production, we would log this and potentially return a friendly error or retry.
                throw new Exception($"AI Generation failed: {ex.Message}", ex);
            }
        }
    }
}
