using OpenAI;
using OpenAI.Chat;

namespace DocumentationCompleteness.Api.Services.AI;

public class AzureOpenAIClientWrapper
{
    private readonly ChatClient _chatClient;
    private readonly int _maxTokens;
    private readonly float _temperature;
    private readonly ILogger<AzureOpenAIClientWrapper> _logger;

    public AzureOpenAIClientWrapper(
        IConfiguration config,
        ILogger<AzureOpenAIClientWrapper> logger)
    {
        _logger      = logger;
        _maxTokens   = int.Parse(config["OpenAI:MaxTokens"] ?? "1500");
        _temperature = float.Parse(config["OpenAI:Temperature"] ?? "0.2");

        var apiKey    = config["OpenAI:ApiKey"]!;
        var modelName = config["OpenAI:DeploymentName"] ?? "gpt-4o";

        var client  = new OpenAIClient(apiKey);
        _chatClient = client.GetChatClient(modelName);
    }

    public async Task<string> GenerateAsync(
        string prompt,
        string? systemPrompt = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Calling OpenAI...");

        var messages = new List<ChatMessage>();
        
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new SystemChatMessage(systemPrompt));
        }
        
        messages.Add(new UserChatMessage(prompt));

        var completion = await _chatClient.CompleteChatAsync(
            messages,
            new ChatCompletionOptions
            {
                MaxOutputTokenCount = _maxTokens,
                Temperature         = _temperature
            },
            ct
        );

        var result = completion.Value.Content[0].Text;
        _logger.LogInformation(
            "OpenAI response received ({Length} chars)", result.Length);
        return result;
    }
}
