using System.Net;
using Microsoft.Extensions.Primitives;

namespace Wanadi.Common.Contracts.RestApi;

public sealed record RestApiFileResponse
{
    public string? MediaType { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: HttpStatusCode retornado pela API
    ///     </para>
    ///     <para>
    ///         en-US: HttpStatusCode returned by the API
    ///     </para>
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: True se o HttpStatusCode estiver no range de 200 - 209, do contrário false.
    ///     </para>
    ///     <para>
    ///         en-US: True if the HttpStatusCode is in the range 200 - 209, otherwise, false.
    ///     </para>
    /// </summary>
    public bool IsSuccessStatusCode { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Cabeçalhos retornados pela API.
    ///     </para>
    ///     <para>
    ///         en-US: Headers returned by the API.
    ///     </para>
    /// </summary>
    public Dictionary<string, StringValues> Headers = new Dictionary<string, StringValues>();

    /// <summary>
    ///     <para>
    ///         pt-BR: Cookies retornados pela API.
    ///     </para>
    ///     <para>
    ///         en-US: Cookies returned by the API.
    ///     </para>
    /// </summary>
    public List<RestApiCookieResponse> Cookies { get; set; } = new List<RestApiCookieResponse>();

    /// <summary>
    ///     <para>
    ///         pt-BR: Caso o HttpStatusCode seja 302 - Recupera dos Headers a Url para redirecionamento (Header: Location)
    ///     </para>
    ///     <para>
    ///         en-US: If the HttpStatusCode is 302 - Retrieves the Url for redirection from the Headers (Header: Location)
    ///     </para>
    /// </summary>
    public string? UrlRedirect => StatusCode != HttpStatusCode.Found ? null : Headers.FirstOrDefault(t => t.Key == "Location").Value.ToString();

    public string? TempFilePath { get; set; }

    public string? FileExtension { get; set; }

    public static async Task<RestApiFileResponse> DownloadResponseAsync(HttpResponseMessage httpResponseMessage, CookieContainer? cookieContainer, CancellationToken cancellationToken = default)
    {
        var response = new RestApiFileResponse();

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
            response.Cookies.AddRange(cookieContainer.GetAllCookies().Select(t => new RestApiCookieResponse(t)).ToList());

        return response;
    }
}