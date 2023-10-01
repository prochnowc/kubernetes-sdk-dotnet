using System.Collections.Generic;

namespace Kubernetes.Generator;

internal sealed class ApiOperationResult
{
    public string Type { get; }

    public IReadOnlyCollection<string> ContentTypes { get; }

    public ApiOperationResult(string type, IReadOnlyCollection<string> contentTypes)
    {
        Type = type;
        ContentTypes = contentTypes;
    }
}