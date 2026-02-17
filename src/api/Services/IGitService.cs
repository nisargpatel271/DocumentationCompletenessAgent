using System;
using System.Threading.Tasks;

namespace DocumentationCompleteness.Api.Services
{
    public interface IGitService
    {
        /// <summary>
        /// Clones a remote repository to a local path.
        /// </summary>
        /// <param name="repositoryUrl">The URL of the remote Git repository.</param>
        /// <param name="localPath">The local directory path to clone into.</param>
        /// <returns>Local path where repo is stored.</returns>
        Task<string> CloneRepositoryAsync(string repositoryUrl, string localPath);
    }
}
