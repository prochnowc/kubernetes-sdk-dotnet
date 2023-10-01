using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Kubernetes.Generator;

internal sealed class ApiOperation
{
    public string Name { get; }

    public string Path { get; }

    public string Method { get; }

    public IReadOnlyCollection<ApiOperationParameter> PathParameters { get; }

    public IReadOnlyCollection<ApiOperationParameter> QueryParameters { get; }

    public IEnumerable<ApiOperationParameter> AllParameters => PathParameters.Concat(QueryParameters);

    public ApiOperationParameter? Body { get; }

    public IReadOnlyCollection<string> Consumes { get; }

    public IReadOnlyCollection<string> Produces { get; }

    public string ResultType { get; }

    public string? Description { get; }

    public ApiOperation(
        string name,
        string path,
        string method,
        IReadOnlyCollection<ApiOperationParameter> pathParameters,
        IReadOnlyCollection<ApiOperationParameter> queryParameters,
        IReadOnlyCollection<string> consumes,
        IReadOnlyCollection<string> produces,
        ApiOperationParameter? body,
        string resultType,
        string? description)
    {
        Name = name;
        Path = path;
        Method = method;
        PathParameters = pathParameters;
        QueryParameters = queryParameters;
        Consumes = consumes;
        Produces = produces;
        Body = body;
        ResultType = resultType;
        Description = description;
    }
}
