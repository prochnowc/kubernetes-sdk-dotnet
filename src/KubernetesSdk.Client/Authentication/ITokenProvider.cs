using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Authentication;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken);
}