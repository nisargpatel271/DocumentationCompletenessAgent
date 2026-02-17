using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Services.Analysis
{
    public interface ICodeAnalyzer
    {
        /// <summary>
        /// Determines if the analyzer supports the given file extension.
        /// </summary>
        bool SupportsFile(string filePath);

        /// <summary>
        /// Analyzes the file and returns a list of documentation gaps.
        /// </summary>
        Task<FileAnalysisResult> AnalyzeFileAsync(string filePath, Guid repositoryId, Guid jobId);
    }
}
