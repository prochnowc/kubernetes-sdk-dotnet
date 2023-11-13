namespace Kubernetes.Generator;

internal sealed class ApiOperationParameter
{
    public string Name { get; }

    public string Type { get; }

    public string ParameterName { get; }

    public string ParameterType => Type + (IsRequired ? string.Empty : "?");

    public bool IsRequired { get; }

    public string? Description { get; }

    public ApiOperationParameter(string name, string type, string parameterName, bool isRequired, string? description)
    {
        Name = name;
        Type = type;
        ParameterName = parameterName;
        IsRequired = isRequired;
        Description = description;
    }
}