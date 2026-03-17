using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services
{
    public interface IAnalysisService
    {
        Task<AnalysisJob> CreateJobAsync(Guid repositoryId);
        Task ExecuteJobAsync(Guid jobId, CancellationToken ct = default);
        Task<List<DocumentationGap>> GetGapsByJobIdAsync(Guid jobId, CancellationToken ct = default);
    }
}
