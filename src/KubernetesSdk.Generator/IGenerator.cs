using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Generator;

internal interface IGenerator
{
    Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default);
}
