using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Generator;

internal sealed class ApiModelGenerator : IGenerator
{
    private const string TemplateFileName = "Model.cs.template";
    private const string OutputFileNameFormat = "{0}.g.cs";

    public async Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default)
    {
        var modelBuilder = new ApiModelBuilder(context);
        var modelRenderer = new TemplateRenderer(TemplateFileName);

        foreach (ApiModel? model in modelBuilder.GetModels())
        {
            string? output = await modelRenderer.RenderAsync(model)
                                                .ConfigureAwait(false);

            await context.AddSourceAsync(string.Format(OutputFileNameFormat, model.Name), output, cancellationToken);
        }
    }
}