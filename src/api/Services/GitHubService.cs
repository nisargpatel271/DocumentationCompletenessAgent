using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Models;
using DocumentationCompleteness.Api.Models.Configuration;
using Microsoft.Extensions.Options;
using Octokit;

namespace DocumentationCompleteness.Api.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;
        private readonly GitHubSettings _settings;

        public GitHubService(IOptions<IntegrationSettings> settings)
        {
            _settings = settings.Value.GitHub;
            _client = new GitHubClient(new ProductHeaderValue("DocumentationCompletenessAgent"));

            if (!string.IsNullOrEmpty(_settings.PersonalAccessToken))
            {
                _client.Credentials = new Credentials(_settings.PersonalAccessToken);
            }
        }

        public async Task<IEnumerable<DocumentationCompleteness.Api.Models.Repository>> GetRepositoriesAsync()
        {
            IReadOnlyList<Octokit.Repository> repos;

            if (!string.IsNullOrEmpty(_settings.Organization))
            {
                repos = await _client.Repository.GetAllForOrg(_settings.Organization);
            }
            else
            {
                repos = await _client.Repository.GetAllForCurrent();
            }

            return repos.Select(MapToModel);
        }

        public async Task<DocumentationCompleteness.Api.Models.Repository?> GetRepositoryAsync(string owner, string name)
        {
            try
            {
                var repo = await _client.Repository.Get(owner, name);
                return MapToModel(repo);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        private DocumentationCompleteness.Api.Models.Repository MapToModel(Octokit.Repository repo)
        {
            return new DocumentationCompleteness.Api.Models.Repository
            {
                Id = Guid.NewGuid(), // Temporary ID, will be replaced if saved or used as key in UI state
                Name = repo.Name,
                Source = "GitHub",
                RepositoryUrl = repo.HtmlUrl,
                DefaultBranch = repo.DefaultBranch,
                IsActive = false, // Not tracked by default
                CreatedBy = repo.Owner.Login, // Using Owner login as CreatedBy for now
                Settings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    GithubId = repo.Id,
                    Owner = repo.Owner.Login,
                    Description = repo.Description,
                    IsPrivate = repo.Private
                })
            };
        }
    }
}
