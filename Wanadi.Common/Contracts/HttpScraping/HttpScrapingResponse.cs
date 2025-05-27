using System.Net;
using Microsoft.Extensions.Primitives;

namespace Wanadi.Common.Contracts.HttpScraping;

public sealed record HttpScrapingResponse
{
    public string? Content { get; set; }
    public string? MediaType { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccessStatusCode { get; set; }
    public Dictionary<string, StringValues> Headers = new Dictionary<string, StringValues>();
    public List<HttpCookieResponse> Cookies { get; set; } = new List<HttpCookieResponse>();
    public string? UrlRedirect => StatusCode != HttpStatusCode.Found ? null : Headers.FirstOrDefault(t => t.Key == "Location").Value.ToString();

    public static async Task<HttpScrapingResponse> ReadResponseAsync(HttpResponseMessage httpResponseMessage, Encoding encoding, CookieContainer? cookieContainer, CancellationToken cancellationToken = default)
    {
        var response = new HttpScrapingResponse();

        response.StatusCode = httpResponseMessage.StatusCode;
        response.MediaType = httpResponseMessage.Content.Headers.ContentType?.MediaType;
        response.IsSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;
        response.Content = await httpResponseMessage.ReadContentToStringAsync(encoding, cancellationToken);

        foreach (var header in httpResponseMessage.Headers)
            response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));

        if (cookieContainer != null && cookieContainer.Count > 0)
            response.Cookies.AddRange(cookieContainer.GetAllCookies().Select(t => new HttpCookieResponse(t)).ToList());

        return response;
    }
}