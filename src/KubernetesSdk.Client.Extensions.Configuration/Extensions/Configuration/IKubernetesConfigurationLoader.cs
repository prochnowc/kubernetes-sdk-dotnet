using System.Collections.Generic;
using System.IO;

namespace Kubernetes.Client.Extensions.Configuration;

public interface IKubernetesConfigurationLoader
{
    IEnumerable<KeyValuePair<string, string>> Load(Stream stream);
}