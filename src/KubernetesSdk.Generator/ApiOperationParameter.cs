namespace Kubernetes.Generator;

internal sealed class ApiOperationParameter
{
    public string Name { get; }

    public string ParameterName { get; }

    public string ParameterType { get; }

    public bool IsRequired { get; }

    public string? Description { get; }

    public ApiOperationParameter(string name, string parameterName, string parameterType, bool isRequired, string? description)
    {
        Name = name;
        ParameterName = parameterName;
        ParameterType = parameterType;
        IsRequired = isRequired;
        Description = description;
    }
}