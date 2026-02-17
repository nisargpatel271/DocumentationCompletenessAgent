using System;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services
{
    public interface IAnalysisService
    {
        /// <summary>
        /// Triggers a full analysis for the specified repository.
        /// </summary>
        /// <param name="repositoryId">The ID of the repository to analyze.</param>
        /// <returns>The created AnalysisJob tracking the process.</returns>
        Task<AnalysisJob> RunAnalysisAsync(Guid repositoryId);
    }
}
