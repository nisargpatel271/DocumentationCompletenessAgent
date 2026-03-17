using System.Threading.Channels;

namespace DocumentationCompleteness.Api.Services.Background
{
    /// <summary>
    /// In-memory job queue using System.Threading.Channels.
    /// Acts as a lightweight message broker between the API controller and the background worker.
    /// Bounded at 100 to provide backpressure if the system is overloaded.
    /// </summary>
    public class AnalysisJobQueue
    {
        private readonly Channel<Guid> _channel =
            Channel.CreateBounded<Guid>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

        public async Task EnqueueAsync(Guid jobId, CancellationToken ct = default)
            => await _channel.Writer.WriteAsync(jobId, ct);

        public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct)
            => _channel.Reader.ReadAllAsync(ct);
    }
}
