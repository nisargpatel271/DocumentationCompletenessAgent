using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services
{
    public interface IGitHubService
    {
        Task<IEnumerable<Repository>> GetRepositoriesAsync();
        Task<Repository?> GetRepositoryAsync(string owner, string name);
    }
}
