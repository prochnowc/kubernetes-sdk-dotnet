using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSwag;

namespace Kubernetes.Generator;

internal sealed class GeneratorExecutionContext
{
    public OpenApiDocument OpenApiDocument { get; }

    public TypeNameResolver TypeNameResolver { get; }

    public string OutputPath { get; }

    public GeneratorExecutionContext(OpenApiDocument openApiDocument, string outputPath)
    {
        OpenApiDocument = openApiDocument;
        TypeNameResolver = new TypeNameResolver(openApiDocument);
        OutputPath = outputPath;
    }

    public async Task AddSourceAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        string filePath = Path.Combine(OutputPath, fileName);

        string? existingContent = null;
        if (File.Exists(filePath))
        {
            existingContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
        }

        if (!string.Equals(existingContent, content))
        {
            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8, cancellationToken);
        }
    }
}