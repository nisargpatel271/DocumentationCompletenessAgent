using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DocumentationCompleteness.Api.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        
        // Extensions to include in analysis
        private readonly HashSet<string> _allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".ts", ".js", ".py"
        };

        // Directories to completely ignore
        private readonly HashSet<string> _ignoredDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git", ".vs", ".idea", "bin", "obj", "node_modules", "dist", "build", "coverage"
        };

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> GetSourceFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.LogWarning("Directory not found: {Path}", directoryPath);
                return Enumerable.Empty<string>();
            }

            var sourceFiles = new List<string>();
            ScanDirectory(directoryPath, sourceFiles);
            return sourceFiles;
        }

        private void ScanDirectory(string dir, List<string> results)
        {
            try
            {
                // Process files in current directory
                foreach (var file in Directory.GetFiles(dir))
                {
                    if (_allowedExtensions.Contains(Path.GetExtension(file)))
                    {
                        results.Add(file);
                    }
                }

                // Recurse into subdirectories
                foreach (var subDir in Directory.GetDirectories(dir))
                {
                    var dirName = Path.GetFileName(subDir);
                    if (!_ignoredDirectories.Contains(dirName))
                    {
                        ScanDirectory(subDir, results);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning directory {Directory}", dir);
            }
        }
    }
}
