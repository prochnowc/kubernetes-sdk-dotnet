using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Generator;

internal sealed class YamlSerializerContextGenerator : IGenerator
{
    private const string TemplateFileName = "KubernetesYamlSerializerContext.cs.template";
    private const string OutputFileName = "KubernetesYamlSerializerContext.g.cs";

    public async Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default)
    {
        var modelBuilder = new ApiModelBuilder(context);
        var jsonRenderer = new TemplateRenderer(TemplateFileName);

        string? output = await jsonRenderer.RenderAsync(modelBuilder.GetModels())
                                           .ConfigureAwait(false);

        await context.AddSourceAsync(OutputFileName, output, cancellationToken);
    }
}