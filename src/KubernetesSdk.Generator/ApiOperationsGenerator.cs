using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Generator;

internal sealed class ApiOperationsGenerator : IGenerator
{
    private const string ClientExtensionsTemplateFileName = "KubernetesClientExtensions.cs.template";
    private const string ClientExtensionsOutputFileNameFormat = "KubernetesClientExtensions.{0}.g.cs";
    private const string ClientOperationsTemplateFileName = "KubernetesClientOperations.cs.template";
    private const string ClientOperationsOutputFileNameFormat = "{0}Operations.g.cs";

    public async Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default)
    {
        var operationsBuilder = new ApiOperationsBuilder(context);

        var clientExtensionsRenderer = new TemplateRenderer(ClientExtensionsTemplateFileName);
        var clientOperationsRenderer = new TemplateRenderer(ClientOperationsTemplateFileName);

        foreach (ApiOperations? operations in operationsBuilder.GetOperations())
        {
            string clientExtensionsOutput =
                await clientExtensionsRenderer.RenderAsync(operations)
                                              .ConfigureAwait(false);

            await context.AddSourceAsync(
                string.Format(ClientExtensionsOutputFileNameFormat, operations.Name),
                clientExtensionsOutput,
                cancellationToken);

            string clientOperationsOutput =
                await clientOperationsRenderer.RenderAsync(operations)
                                              .ConfigureAwait(false);

            await context.AddSourceAsync(
                string.Format(ClientOperationsOutputFileNameFormat, operations.Name),
                clientOperationsOutput,
                cancellationToken);
        }
    }
}
