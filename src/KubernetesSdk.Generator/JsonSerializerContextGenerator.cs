using System.Threading;
using System.Threading.Tasks;
using NSwag;

namespace Kubernetes.Generator;

internal sealed class JsonSerializerContextGenerator : IGenerator
{
    public async Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default)
    {
        var modelBuilder = new ApiModelBuilder(context.OpenApiDocument);
        var jsonRenderer = new TemplateRenderer("KubernetesJsonSerializerContext.cs.template");

        string? output = await jsonRenderer.RenderAsync(modelBuilder.GetModels())
                                            .ConfigureAwait(false);

        await context.AddSourceAsync($"KubernetesJsonSerializerContext.g.cs", output, cancellationToken);
    }
}