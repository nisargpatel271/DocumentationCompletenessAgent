namespace DocumentationCompleteness.Api.Models.Configuration
{
    public class IntegrationSettings
    {
        public GitHubSettings GitHub { get; set; } = new GitHubSettings();
        public AzureDevOpsSettings AzureDevOps { get; set; } = new AzureDevOpsSettings();
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
