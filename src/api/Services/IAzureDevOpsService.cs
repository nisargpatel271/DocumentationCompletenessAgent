using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services
{
    public interface IAzureDevOpsService
    {
        Task<IEnumerable<Repository>> GetRepositoriesAsync();
    }
}
