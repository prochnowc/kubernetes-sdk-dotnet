using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Kubernetes.Generator;

internal sealed class ApiOperation
{
    public string Name { get; }

    public string Path { get; }

    public string Method { get; }

    public GroupVersionKind? GroupVersionKind { get; }

    public string? Action { get; }

    public IReadOnlyCollection<ApiOperationParameter> PathParameters { get; }

    public IReadOnlyCollection<ApiOperationParameter> QueryParameters { get; }

    public IEnumerable<ApiOperationParameter> AllParameters
    {
        get
        {
            IEnumerable<ApiOperationParameter> all = PathParameters;
            if (Body != null)
                all = all.Concat(new[] { Body });

            all = all.Concat(QueryParameters);
            return all;
        }
    }

    public ApiOperationParameter? Body { get; }

    public IReadOnlyCollection<string> Consumes { get; }

    public IReadOnlyCollection<string> Produces { get; }

    public string ResultType { get; }

    public string? Description { get; }

    public ApiOperation(
        string name,
        string path,
        string method,
        GroupVersionKind? groupVersionKind,
        string? action,
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
        GroupVersionKind = groupVersionKind;
        Action = action;
        PathParameters = pathParameters;
        QueryParameters = queryParameters;
        Consumes = consumes;
        Produces = produces;
        Body = body;
        ResultType = resultType;
        Description = description;
    }
}
