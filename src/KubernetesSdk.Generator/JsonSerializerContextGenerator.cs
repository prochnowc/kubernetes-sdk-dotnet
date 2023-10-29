using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Generator;

internal sealed class JsonSerializerContextGenerator : IGenerator
{
    private const string TemplateFileName = "KubernetesJsonSerializerContext.cs.template";
    private const string OutputFileName = "KubernetesJsonSerializerContext.g.cs";

    public async Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default)
    {
        var modelBuilder = new ApiModelBuilder(context);
        var jsonRenderer = new TemplateRenderer(TemplateFileName);

        string? output = await jsonRenderer.RenderAsync(modelBuilder.GetModels())
                                           .ConfigureAwait(false);

        await context.AddSourceAsync(OutputFileName, output, cancellationToken);
    }
}