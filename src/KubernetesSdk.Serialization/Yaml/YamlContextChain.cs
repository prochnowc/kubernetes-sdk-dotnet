using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectFactories;

namespace Kubernetes.Serialization.Yaml;

internal sealed class YamlContextChain : StaticContext
{
    // TODO: Implement inspecting all context's
    private readonly IEnumerable<StaticContext> _contexts;

    public YamlContextChain(IEnumerable<StaticContext> contexts)
    {
        _contexts = contexts;
    }

    public override StaticObjectFactory GetFactory()
    {
        return _contexts.First()
                        .GetFactory();
    }

    public override ITypeInspector GetTypeInspector()
    {
        return _contexts.First()
                        .GetTypeInspector();
    }
}