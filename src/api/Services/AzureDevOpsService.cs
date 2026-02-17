using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace DocumentationCompleteness.Api.Services
{
    public class AzureDevOpsService : IAzureDevOpsService
    {
        private readonly AzureDevOpsSettings _settings;

        public AzureDevOpsService(IOptions<IntegrationSettings> settings)
        {
            _settings = settings.Value.AzureDevOps;
        }

        public async Task<IEnumerable<Repository>> GetRepositoriesAsync()
        {
            if (string.IsNullOrEmpty(_settings.OrganizationUrl) || string.IsNullOrEmpty(_settings.PersonalAccessToken))
            {
                return Enumerable.Empty<Repository>();
            }

            var credentials = new VssBasicCredential(string.Empty, _settings.PersonalAccessToken);
            var connection = new VssConnection(new Uri(_settings.OrganizationUrl), credentials);

            using var gitClient = connection.GetClient<GitHttpClient>();
            
            // Get all repositories in the organization (across all projects)
            var repos = await gitClient.GetRepositoriesAsync();

            return repos.Select(MapToModel);
        }

        private Repository MapToModel(GitRepository repo)
        {
            return new Repository
            {
                Id = Guid.NewGuid(), // Temporary ID
                Name = repo.Name,
                Source = "AzureDevOps",
                RepositoryUrl = repo.RemoteUrl,
                DefaultBranch = repo.DefaultBranch?.Replace("refs/heads/", "") ?? "main",
                IsActive = false,
                CreatedBy = repo.ProjectReference?.Name ?? "Unknown", // Using Project Name as a grouper/creator context
                Settings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    AdoId = repo.Id,
                    ProjectId = repo.ProjectReference?.Id,
                    ProjectName = repo.ProjectReference?.Name,
                    SshUrl = repo.SshUrl,
                    WebUrl = repo.WebUrl
                })
            };
        }
    }
}
