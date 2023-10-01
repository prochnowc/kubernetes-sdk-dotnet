using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Generator;

internal sealed class ApiModelGenerator : IGenerator
{
    public async Task GenerateAsync(GeneratorExecutionContext context, CancellationToken cancellationToken = default)
    {
        var modelBuilder = new ApiModelBuilder(context.OpenApiDocument);
        var modelRenderer = new TemplateRenderer("ApiModel.cs.template");

        foreach (ApiModel? model in modelBuilder.GetModels())
        {
            string? output = await modelRenderer.RenderAsync(model)
                                                .ConfigureAwait(false);

            await context.AddSourceAsync($"{model.Type}.g.cs", output, cancellationToken);
        }
    }
}