using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSwag;

namespace Kubernetes.Generator;

public static class Program
{
    public static async Task Main(string[] args)
    {
        string swaggerJsonPath = args[0];
        string outputPath = args[1];

        List<IGenerator> generators = new ();

        foreach (string generatorName in args[2].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            switch (generatorName)
            {
                case "model":
                    generators.Add(new ApiModelGenerator());
                    break;

                case "api":
                    generators.Add(new ApiOperationsGenerator());
                    break;

                case "jsoncontext":
                    generators.Add(new JsonSerializerContextGenerator());
                    break;

                case "yamlcontext":
                    generators.Add(new YamlSerializerContextGenerator());
                    break;

                default:
                    throw new ApplicationException($"Unknown code generator {generatorName}");
            }
        }

        OpenApiDocument openApiDocument =
            await OpenApiDocument.FromFileAsync(swaggerJsonPath)
                                 .ConfigureAwait(false);

        var context = new GeneratorExecutionContext(openApiDocument, outputPath);

        foreach (IGenerator generator in generators)
        {
            await generator.GenerateAsync(context)
                           .ConfigureAwait(false);
        }
    }
}