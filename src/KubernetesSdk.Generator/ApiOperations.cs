using System.Collections.Generic;

namespace Kubernetes.Generator;

internal sealed class ApiOperations
{
    public string Name { get; }

    public IReadOnlyCollection<ApiOperation> Methods { get; }

    public ApiOperations(string name, IReadOnlyCollection<ApiOperation> methods)
    {
        Name = name;
        Methods = methods;
    }
}
