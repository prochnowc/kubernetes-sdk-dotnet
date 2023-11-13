using System.Collections.Generic;

namespace Kubernetes.Generator;

internal sealed class ApiModelProperty
{
    private static readonly HashSet<string> ValueTypes = new ()
    {
        "bool",
        "char",
        "int",
        "uint",
        "byte",
        "sbyte",
        "short",
        "ushort",
        "long",
        "ulong",
        "float",
        "double",
        "decimal",
        "global::System.DateTime",
        "global::System.DateTimeOffset",
        "global::System.TimeSpan",
    };

    public string Name { get; }

    public string Type { get; }

    public string PropertyName { get; }

    public string PropertyType => Type + (IsRequired ? string.Empty : "?");

    public string ParameterName { get; }

    public string ParameterType => Type + (IsRequired ? string.Empty : "?");

    public bool IsRequired { get; }

    public bool IsValueType => ValueTypes.Contains(Type);

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
