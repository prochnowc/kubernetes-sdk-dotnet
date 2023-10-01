using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSwag;

namespace Kubernetes.Generator;

internal sealed class GeneratorExecutionContext
{
    public OpenApiDocument OpenApiDocument { get; }

    public string OutputPath { get; }

    public GeneratorExecutionContext(OpenApiDocument openApiDocument, string outputPath)
    {
        OpenApiDocument = openApiDocument;
        OutputPath = outputPath;
    }

    public async Task AddSourceAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

#if NETSTANDARD2_0
        File.WriteAllText(Path.Combine(OutputPath, fileName), content, Encoding.UTF8);
#else
        await File.WriteAllTextAsync(Path.Combine(OutputPath, fileName), content, Encoding.UTF8, cancellationToken);
#endif
    }
}