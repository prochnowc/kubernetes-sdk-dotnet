using System.Net.Http;

namespace Kubernetes.Client;

internal static class HttpMethods
{
    public static readonly HttpMethod Head = HttpMethod.Head;

    public static readonly HttpMethod Get = HttpMethod.Get;

    public static readonly HttpMethod Delete = HttpMethod.Delete;

    public static readonly HttpMethod Post = HttpMethod.Post;

    public static readonly HttpMethod Put = HttpMethod.Put;

#if NETSTANDARD2_0 || NET40_OR_GREATER
    public static readonly HttpMethod Patch = new HttpMethod("PATCH");
#else
    public static readonly HttpMethod Patch = HttpMethod.Patch;
#endif
}
