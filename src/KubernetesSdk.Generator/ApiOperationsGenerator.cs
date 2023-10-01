using System.Threading;
using System.Threading.Tasks;
using NSwag;

namespace Kubernetes.Generator;

internal sealed class ApiOperationsGenerator : IGenerator
{
    public async Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default)
    {
        var operationsBuilder = new ApiOperationsBuilder(context.OpenApiDocument);

        var clientRenderer = new TemplateRenderer("KubernetesClientExtensions.cs.template");
        var operationsRenderer = new TemplateRenderer("KubernetesClientOperations.cs.template");

        foreach (ApiOperations? operations in operationsBuilder.GetOperations())
        {
            string? clientOutput = await clientRenderer.RenderAsync(operations)
                                                       .ConfigureAwait(false);

            await context.AddSourceAsync(
                $"KubernetesClientExtensions.{operations.Name}.g.cs",
                clientOutput,
                cancellationToken);

            string? output = await operationsRenderer.RenderAsync(operations)
                                                     .ConfigureAwait(false);

            await context.AddSourceAsync($"{operations.Name}Operations.g.cs", output, cancellationToken);
        }
    }
}
