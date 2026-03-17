namespace DocumentationCompleteness.Api.Models.Configuration
{
    public class IntegrationSettings
    {
        public GitHubSettings GitHub { get; set; } = new GitHubSettings();
        public AzureDevOpsSettings AzureDevOps { get; set; } = new AzureDevOpsSettings();
        public AzureOpenAISettings AzureOpenAI { get; set; } = new AzureOpenAISettings();
    }

    public class AzureOpenAISettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string DeploymentName { get; set; } = "gpt-4-turbo";
    }

    public class GitHubSettings
    {
        public string PersonalAccessToken { get; set; } = string.Empty;
        public string? Organization { get; set; }
    }

    public class AzureDevOpsSettings
    {
        public string PersonalAccessToken { get; set; } = string.Empty;
        public string? OrganizationUrl { get; set; }
    }
}
