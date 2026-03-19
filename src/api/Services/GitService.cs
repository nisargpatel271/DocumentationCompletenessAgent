using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DocumentationCompleteness.Api.Services
{
    public class GitService : IGitService
    {
        private readonly ILogger<GitService> _logger;

        public GitService(ILogger<GitService> logger)
        {
            _logger = logger;
        }

        public async Task<string> CloneRepositoryAsync(string repositoryUrl, string localPath)
        {
            if (string.IsNullOrWhiteSpace(repositoryUrl))
                throw new ArgumentException("Repository URL cannot be null or empty.", nameof(repositoryUrl));

            // Normalize URL: Ensure .git suffix if it's a standard HTTPS repo URL
            if (!repositoryUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase) && 
                (repositoryUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                 repositoryUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                repositoryUrl += ".git";
            }

            if (Directory.Exists(localPath))
            {
                _logger.LogInformation("Repository already exists at {LocalPath}. Cleaning up...", localPath);
                DeleteDirectory(localPath);
            }

            // Small delay to ensure OS releases locks
            await Task.Delay(100);
            Directory.CreateDirectory(localPath);

            _logger.LogInformation("Cloning {RepositoryUrl} to {LocalPath}...", repositoryUrl, localPath);

            // Using simple 'git' command line execution
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"clone \"{repositoryUrl}\" .",
                WorkingDirectory = localPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            
            var errorOutput = new System.Text.StringBuilder();
            process.OutputDataReceived += (sender, e) => _logger.LogDebug(e.Data);
            process.ErrorDataReceived += (sender, e) => 
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput.AppendLine(e.Data);
                    _logger.LogWarning(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var errorMsg = errorOutput.ToString();
                _logger.LogError("Git clone failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, errorMsg);

                if (errorMsg.Contains("Authentication failed", StringComparison.OrdinalIgnoreCase) ||
                    errorMsg.Contains("could not read Username", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Authentication failed. Please verify your credentials/token.");
                }
                
                if (errorMsg.Contains("repository not found", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Repository not found. Verify the URL is correct and public, or providing a valid token.");
                }

                throw new Exception($"Git clone failed ({process.ExitCode}): {errorMsg}");
            }

            _logger.LogInformation("Successfully cloned repository.");
            return localPath;
        }

        private void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }
    }
}
