namespace Kubernetes.Generator;

internal sealed class ApiModelProperty
{
    public string Name { get; }

    public string PropertyName { get; }

    public string ParameterName { get; }

    public string Type { get; }

    public bool IsRequired { get; }

    public string? Description { get; }

    public ApiModelProperty(
        string name,
        string type,
        string propertyName,
        string parameterName,
        bool required,
        string? description)
    {
        Name = name;
        PropertyName = propertyName;
        ParameterName = parameterName;
        Type = type;
        IsRequired = required;
        Description = description;
    }
}
