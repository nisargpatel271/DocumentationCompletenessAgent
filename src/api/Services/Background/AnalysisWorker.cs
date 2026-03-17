using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DocumentationCompleteness.Api.Services.Background
{
    public class AnalysisWorker : BackgroundService
    {
        private readonly AnalysisJobQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AnalysisWorker> _logger;

        public AnalysisWorker(
            AnalysisJobQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<AnalysisWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AnalysisWorker started.");

            try
            {
                await foreach (var jobId in _queue.DequeueAllAsync(stoppingToken))
                {
                    try
                    {
                        _logger.LogInformation("Starting job {JobId}", jobId);

                        await using var scope = _scopeFactory.CreateAsyncScope();
                        var analysisService = scope.ServiceProvider
                            .GetRequiredService<IAnalysisService>();

                        await analysisService.ExecuteJobAsync(jobId, stoppingToken);

                        _logger.LogInformation("Completed job {JobId}", jobId);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Job {JobId} failed", jobId);
                    }
                }
            }
            catch (OperationCanceledException) { }

            _logger.LogInformation("AnalysisWorker stopped.");
        }
    }
}
