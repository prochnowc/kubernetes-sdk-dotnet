using System;
using System.Linq;
using System.Text;
using Scriban.Functions;

namespace Kubernetes.Generator;

internal sealed class TemplateFunctions
{
    public static string XmlDoc(string? text, string tag, string? attributes = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        StringBuilder sb = new ();
        sb.Append("/// <");
        sb.Append(tag);
        if (!string.IsNullOrWhiteSpace(attributes))
        {
            sb.Append(' ');
            sb.Append(attributes);
        }

        sb.AppendLine(">");

        foreach (string? line in lines)
        {
            sb.Append("/// ");
            sb.AppendLine(HtmlFunctions.Escape(line));
        }

        sb.Append("/// </");
        sb.Append(tag);
        sb.AppendLine(">");

        return sb.ToString();
    }

    public static string DotnetReturnType(ApiOperation operation)
    {
        if (operation.Name.StartsWith("Watch"))
            return $"IWatcher<{operation.ResultType}>";

        return operation.ResultType;
    }

    public static string DotnetAsyncReturnType(ApiOperation operation)
    {
        string returnType = DotnetReturnType(operation);
        if (returnType == "void")
            return "Task";

        return $"Task<{returnType}>";
    }

    private static void DotnetFormatParametersCore(StringBuilder sb, ApiOperation operation)
    {
        foreach (ApiOperationParameter parameter in operation.PathParameters.Concat(
                     new[] { operation.Body }.Concat(operation.QueryParameters)
                                             .Where(p => p != null)
                                             .Select(p => p!)))
        {
            if (sb.Length > 0)
            {
                sb.AppendLine(",");
            }

            sb.Append(parameter.ParameterType);
            sb.Append(' ');
            sb.Append(parameter.ParameterName);

            if (!parameter.IsRequired)
                sb.Append(" = default");
        }
    }

    public static string DotnetFormatParameters(ApiOperation operation)
    {
        StringBuilder sb = new ();
        DotnetFormatParametersCore(sb, operation);
        return sb.ToString();
    }

    public static string DotnetFormatAsyncParameters(ApiOperation operation)
    {
        StringBuilder sb = new ();
        DotnetFormatParametersCore(sb, operation);

        if (sb.Length > 0)
            sb.AppendLine(",");

        sb.Append("CancellationToken cancellationToken = default");

        return sb.ToString();
    }

    public static string DotnetFormatBaseTypes(ApiModel model)
    {
        StringBuilder sb = new ();

        foreach (string @interface in model.Interfaces)
        {
            sb.Append(sb.Length == 0 ? ": " : ", ");
            sb.Append(@interface);
        }

        return sb.ToString();
    }
}
