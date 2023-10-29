using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using CaseExtensions;
using NJsonSchema;
using NSwag;

namespace Kubernetes.Generator;

internal sealed class ApiOperationsBuilder
{
    private const string ListMethodPrefix = "List";
    private const string EnumerateMethodPrefix = "Enumerate";
    private const string WatchMethodPrefix = "Watch";

    private static readonly HashSet<string> ListParameterFilter = new ()
    {
        "allowWatchBookmarks",
        "timeoutSeconds",
        "watch",
        "sendInitialEvents",
    };

    private static readonly HashSet<string> EnumerateParameterFilter = new ()
    {
        "allowWatchBookmarks",
        "timeoutSeconds",
        "watch",
        "sendInitialEvents",
        "limit",
        "continue",
    };

    private static readonly HashSet<string> WatchParameterFilter = new ()
    {
        "watch",
        "limit",
        "continue",
    };

    private static readonly HashSet<string> CustomObjectsParameterFilter = new ()
    {
        "group",
        "version",
        "plural",
    };

    private readonly GeneratorExecutionContext _context;

    public ApiOperationsBuilder(GeneratorExecutionContext context)
    {
        _context = context;
    }

    public IEnumerable<ApiOperations> GetOperations()
    {
        Dictionary<string, List<OpenApiOperationDescription>>? operationsGroupedByTag =
            GetOperationsGroupedByTag(_context.OpenApiDocument.Operations);

        foreach (KeyValuePair<string, List<OpenApiOperationDescription>> operationsGroup in operationsGroupedByTag)
        {
            string groupName = operationsGroup.Key.ToPascalCase();

            var methods = new List<ApiOperation>();
            foreach (OpenApiOperationDescription? o in operationsGroup.Value)
            {
                string methodName = o.Operation.OperationId.ToPascalCase();
                string methodPath = o.Path.TrimStart('/');

                (string Name, OpenApiParameter Parameter)[] parameters = GetParametersWithName(o);

                ApiOperationParameter[] pathParameters = GetMethodParameters(parameters, OpenApiParameterKind.Path);
                ApiOperationParameter[] queryParameters = GetMethodParameters(parameters, OpenApiParameterKind.Query);

                OpenApiParameter? body = o.Operation.ActualParameters
                                          .SingleOrDefault(p => p.Kind == OpenApiParameterKind.Body);

                ApiOperationParameter? bodyParameter = body != null
                    ? new ApiOperationParameter(
                        body.Name,
                        NameTransformer.GetParameterName(body.Name),
                        _context.TypeNameResolver.GetParameterTypeName(o.Operation, body),
                        body.IsRequired,
                        body.Description)
                    : null;

                string resultType = _context.TypeNameResolver.GetResponseTypeName(o.Operation);

                // filter path parameters for custom objects which are set by the KubernetesEntityAttribute
                if (groupName == "CustomObjects")
                {
                    pathParameters = pathParameters.Where(p => !CustomObjectsParameterFilter.Contains(p.Name))
                                                   .ToArray();
                }

                if (methodName.StartsWith(ListMethodPrefix))
                {
                    methods.Add(
                        new ApiOperation(
                            methodName,
                            methodPath,
                            o.Method.ToPascalCase(),
                            pathParameters,
                            queryParameters.Where(p => !ListParameterFilter.Contains(p.Name))
                                           .ToArray(),
                            o.Operation.ActualConsumes.ToArray(),
                            o.Operation.ActualProduces.ToArray(),
                            bodyParameter,
                            resultType,
                            o.Operation.Description));

                    /*
                    methods.Add(
                        new ApiOperation(
                            EnumerateMethodPrefix + methodName.Substring(ListMethodPrefix.Length),
                            methodPath,
                            o.Method.ToPascalCase(),
                            pathParameters,
                            queryParameters.Where(p => !EnumerateParameterFilter.Contains(p.Name))
                                           .ToArray(),
                            o.Operation.ActualConsumes.ToArray(),
                            o.Operation.ActualProduces.ToArray(),
                            bodyParameter,
                            resultType));
                    */

                    string watchResultType = resultType;
                    if (watchResultType.EndsWith("List"))
                    {
                        watchResultType = watchResultType.Substring(0, resultType.Length - "List".Length);
                    }
                    else if (watchResultType.Equals("KubernetesList<T>"))
                    {
                        watchResultType = "T";
                    }

                    methods.Add(
                        new ApiOperation(
                            WatchMethodPrefix + methodName.Substring(ListMethodPrefix.Length),
                            methodPath,
                            o.Method.ToPascalCase(),
                            pathParameters,
                            queryParameters.Where(p => !WatchParameterFilter.Contains(p.Name))
                                           .ToArray(),
                            o.Operation.ActualConsumes.ToArray(),
                            o.Operation.ActualProduces.ToArray(),
                            bodyParameter,
                            watchResultType,
                            o.Operation.Description));
                }
                else
                {
                    methods.Add(
                        new ApiOperation(
                            methodName,
                            methodPath,
                            o.Method.ToPascalCase(),
                            pathParameters,
                            queryParameters,
                            o.Operation.ActualConsumes.ToArray(),
                            o.Operation.ActualProduces.ToArray(),
                            bodyParameter,
                            resultType,
                            o.Operation.Description));
                }
            }

            yield return new ApiOperations(groupName, methods);
        }
    }

    private Dictionary<string, List<OpenApiOperationDescription>> GetOperationsGroupedByTag(
        IEnumerable<OpenApiOperationDescription> operations)
    {
        Dictionary<string, List<OpenApiOperationDescription>> result = new ();

        foreach (OpenApiOperationDescription? operation in operations.Where(
                     o => o.Method != OpenApiOperationMethod.Options))
        {
            /*if (!operation.Operation.TryGetKubernetesGroupVersionKind(out GroupVersionKind? groupVersionKind))
                continue;

            string groupName = (groupVersionKind.Kind + "_" + groupVersionKind.Version).ToPascalCase();

            if (!result.TryGetValue(groupName, out List<OpenApiOperationDescription>? taggedOperations))
            {
                taggedOperations = new List<OpenApiOperationDescription>();
                result.Add(groupName, taggedOperations);
            }

            taggedOperations.Add(operation);
            */

            foreach (string tag in operation.Operation.Tags)
            {
                if (!result.TryGetValue(tag, out List<OpenApiOperationDescription>? taggedOperations))
                {
                    taggedOperations = new List<OpenApiOperationDescription>();
                    result.Add(tag, taggedOperations);
                }

                taggedOperations.Add(operation);
            }
        }

        return result;
    }

    private (string Name, OpenApiParameter Parameter)[] GetParametersWithName(OpenApiOperationDescription o)
    {
        (string Name, OpenApiParameter Parameter)[] parameters =
            o
                .Operation.ActualParameters.Select(
                    p => (p.Name, Parameter: p))
                .ToArray();

        var parameterNames = new HashSet<string>();

        for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
        {
            string? parameterName = parameters[parameterIndex].Name;

            int i = 1;
            while (parameterNames.Contains(parameterName))
            {
                i++;
                parameterName = parameters[parameterIndex].Name + i;
            }

            parameterNames.Add(parameterName);
            parameters[parameterIndex].Name = parameterName;
        }

        return parameters;
    }

    private ApiOperationParameter[] GetMethodParameters(
        (string Name, OpenApiParameter Parameter)[] parameters,
        OpenApiParameterKind kind)
    {
        return parameters.Where(p => p.Parameter.Kind == kind)
                         .OrderBy(p => !p.Parameter.IsRequired)
                         .Select(
                             p => new ApiOperationParameter(
                                 p.Parameter.Name,
                                 NameTransformer.GetParameterName(p.Name),
                                 _context.TypeNameResolver.GetParameterTypeName((OpenApiOperation)p.Parameter.Parent, p.Parameter),
                                 p.Parameter.IsRequired,
                                 p.Parameter.Description))
                         .ToArray();
    }
}
