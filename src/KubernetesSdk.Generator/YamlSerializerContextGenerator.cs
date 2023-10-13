using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Generator;

internal sealed class YamlSerializerContextGenerator : IGenerator
{
    public async Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default)
    {
        var modelBuilder = new ApiModelBuilder(context.OpenApiDocument);
        var jsonRenderer = new TemplateRenderer("KubernetesYamlSerializerContext.cs.template");

        string? output = await jsonRenderer.RenderAsync(modelBuilder.GetModels())
                                           .ConfigureAwait(false);

        await context.AddSourceAsync($"KubernetesYamlSerializerContext.g.cs", output, cancellationToken);
    }
}