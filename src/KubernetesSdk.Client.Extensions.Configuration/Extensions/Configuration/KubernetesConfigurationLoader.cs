using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Kubernetes.Client.Extensions.Configuration;

public abstract class KubernetesConfigurationLoader : IKubernetesConfigurationLoader
{
    protected abstract IConfigurationProvider LoadCore(Stream stream);

    public IEnumerable<KeyValuePair<string, string>> Load(Stream stream)
    {
        IConfigurationProvider provider = LoadCore(stream);
        try
        {
            var keys = new Stack<string?>();
            keys.Push(null);

            while (keys.Count > 0)
            {
                string? parentKey = keys.Pop();
                foreach (string childKey in provider.GetChildKeys(Enumerable.Empty<string>(), parentKey))
                {
                    string key = parentKey is null
                        ? childKey
                        : $"{parentKey}:{childKey}";

                    if (provider.TryGet(key, out string value))
                    {
                        yield return new KeyValuePair<string, string>(key, value);
                    }

                    keys.Push(key);
                }
            }
        }
        finally
        {
            (provider as IDisposable)?.Dispose();
        }
    }
}