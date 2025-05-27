using System.Net;
using Microsoft.Extensions.Primitives;

namespace Wanadi.Common.Contracts.HttpScraping;

public sealed record HttpScrapingFileResponse
{
    public string? MediaType { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccessStatusCode { get; set; }
    public Dictionary<string, StringValues> Headers = new Dictionary<string, StringValues>();
    public List<HttpCookieResponse> Cookies { get; set; } = new List<HttpCookieResponse>();
    public string? UrlRedirect => StatusCode != HttpStatusCode.Found ? null : Headers.FirstOrDefault(t => t.Key == "Location").Value.ToString();
    public string? TempFilePath { get; set; }
    public string? FileExtension { get; set; }

    public static async Task<HttpScrapingFileResponse> DownloadResponseAsync(HttpResponseMessage httpResponseMessage, CookieContainer? cookieContainer, CancellationToken cancellationToken = default)
    {
        var response = new HttpScrapingFileResponse();

        response.StatusCode = httpResponseMessage.StatusCode;
        response.MediaType = httpResponseMessage.Content.Headers.ContentType?.MediaType;
        response.IsSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;
        response.TempFilePath = Path.GetTempFileName();
        response.FileExtension = httpResponseMessage.GetExtensionFromResponse();

        if (File.Exists(response.TempFilePath))
            File.Delete(response.TempFilePath);

        using (var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken))
        using (var fileStream = new FileStream(response.TempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
        {
            await contentStream.CopyToAsync(fileStream);
        }

        foreach (var header in httpResponseMessage.Headers)
            response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));

        if (cookieContainer is { Count: > 0 })
            response.Cookies.AddRange(cookieContainer.GetAllCookies().Select(t => new HttpCookieResponse(t)).ToList());

        return response;
    }
}