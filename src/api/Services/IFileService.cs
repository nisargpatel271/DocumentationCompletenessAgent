using System.Collections.Generic;

namespace DocumentationCompleteness.Api.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Recursively retrieves all source code files from lengths directory, skipping ignored folders.
        /// </summary>
        /// <param name="directoryPath">The root directory to scan.</param>
        /// <returns>A list of absolute file paths.</returns>
        IEnumerable<string> GetSourceFiles(string directoryPath);
    }
}
