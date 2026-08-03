using System.Collections.Concurrent;
using System.Net;

namespace Wanadi.Common.Contracts.Configurations;

public sealed class HttpClientCookieContainerProvider
{
    private const string DefaultClientName = "";

    private readonly ConcurrentDictionary<string, CookieContainer> _cookieContainers = new();

    public CookieContainer Get(string? httpClientName)
    {
        var key = string.IsNullOrWhiteSpace(httpClientName)
            ? DefaultClientName
            : httpClientName;

        return _cookieContainers.GetOrAdd(key, _ => new CookieContainer());
    }
}
