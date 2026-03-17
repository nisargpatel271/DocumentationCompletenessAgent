using System.Threading;
using System.Threading.Tasks;

namespace DocumentationCompleteness.Api.Services.AI
{
    public interface IAIService
    {
        Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default);
    }
}
