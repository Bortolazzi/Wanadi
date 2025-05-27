using System.Net;
using Wanadi.Common.Contracts.HttpScraping;

namespace Wanadi.Common.Clients;

public abstract class HttpScrapingClient : IDisposable
{
    protected Encoding DefaultEncoding { get; set; }
    protected readonly HttpClient _httpClient;
    protected readonly CookieContainer _cookieContainer;

    public HttpScrapingClient(IHttpClientFactory _httpClientFactory, CookieContainer cookieContainer, string? httpClientName = null) : this(_httpClientFactory, cookieContainer, Encoding.UTF8, httpClientName) { }

    public HttpScrapingClient(IHttpClientFactory _httpClientFactory, CookieContainer cookieContainer, Encoding defaultEncoding, string? httpClientName = null)
    {
        DefaultEncoding = defaultEncoding;
        _cookieContainer = cookieContainer;
        _httpClient = httpClientName is not { Length: > 0 } ? _httpClientFactory.CreateClient() : _httpClientFactory.CreateClient(httpClientName);
    }

    protected virtual async Task<HttpScrapingResponse> GetAsync(string fullUriOrPathUri, Dictionary<string, string>? headers = null, Encoding? encoding = null, CancellationToken cancellationToken = default)
        => await ExecuteAsync(fullUriOrPathUri, HttpMethod.Get, null, headers, encoding, cancellationToken);

    protected virtual async Task<HttpScrapingResponse> PostAsync(string fullUriOrPathUri, Dictionary<string, string>? headers = null, Encoding? encoding = null, CancellationToken cancellationToken = default)
        => await ExecuteAsync(fullUriOrPathUri, HttpMethod.Post, null, headers, encoding, cancellationToken);

    protected virtual async Task<HttpScrapingResponse> PostAsync(string fullUriOrPathUri, object? requestContent = null, Dictionary<string, string>? headers = null, Encoding? encoding = null, CancellationToken cancellationToken = default)
        => await ExecuteAsync(fullUriOrPathUri, HttpMethod.Post, requestContent, headers, encoding, cancellationToken);

    protected virtual async Task<HttpScrapingFileResponse> GetDownloadAsync(string fullUriOrPathUri, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => await ExecuteDownloadAsync(fullUriOrPathUri, HttpMethod.Get, null, headers, cancellationToken);

    protected virtual async Task<HttpScrapingFileResponse> PostDownloadAsync(string fullUriOrPathUri, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => await ExecuteDownloadAsync(fullUriOrPathUri, HttpMethod.Post, null, headers, cancellationToken);

    protected virtual async Task<HttpScrapingFileResponse> PostDownloadAsync(string fullUriOrPathUri, object? requestContent = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => await ExecuteDownloadAsync(fullUriOrPathUri, HttpMethod.Post, requestContent, headers, cancellationToken);

    protected virtual async Task<HttpScrapingResponse> ExecuteAsync(string fullUriOrPathUri, HttpMethod method, object? requestContent = null, Dictionary<string, string>? headers = null, Encoding? encoding = null, CancellationToken cancellationToken = default)
        => await SendAndReadResponseAsync(fullUriOrPathUri, method, requestContent, headers, encoding, cancellationToken);

    protected virtual async Task<HttpScrapingFileResponse> ExecuteDownloadAsync(string fullUriOrPathUri, HttpMethod method, object? requestContent = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => await SendAndDownloadResponseAsync(fullUriOrPathUri, method, requestContent, headers, cancellationToken);

    private async Task<HttpScrapingResponse> SendAndReadResponseAsync(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? requestContent,
        Dictionary<string, string>? headers,
        Encoding? encoding,
        CancellationToken cancellationToken)
    {
        using (var httpResponseMessage = await SendAsync(fullUriOrPathUri, httpMethod, requestContent, headers, cancellationToken))
            return await HttpScrapingResponse.ReadResponseAsync(httpResponseMessage, encoding ?? DefaultEncoding, _cookieContainer, cancellationToken);
    }

    private async Task<HttpScrapingFileResponse> SendAndDownloadResponseAsync(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? requestContent,
        Dictionary<string, string>? headers,
        CancellationToken cancellationToken)
    {
        using (var httpResponseMessage = await SendAsync(fullUriOrPathUri, httpMethod, requestContent, headers, cancellationToken))
            return await HttpScrapingFileResponse.DownloadResponseAsync(httpResponseMessage, _cookieContainer, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? requestContent,
        Dictionary<string, string>? headers,
        CancellationToken cancellationToken)
    {
        using (var _message = BuildRequestMessage(fullUriOrPathUri, httpMethod, requestContent, headers))
            return await _httpClient.SendAsync(_message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private HttpRequestMessage BuildRequestMessage(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? requestContent,
        Dictionary<string, string>? headers)
    {
        var response = new HttpRequestMessage(httpMethod, GetUriToRequest(fullUriOrPathUri));

        if (headers is { Count: > 0 })
        {
            foreach (var header in headers)
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (httpMethod == HttpMethod.Get || requestContent == null)
            return response;

        response.Content = BuildRequestContent(requestContent!);

        return response;
    }

    private FormUrlEncodedContent BuildRequestContent(object requestContent)
    {
        var requestContentType = requestContent.GetType();

        if (requestContentType.IsGenericType &&
            requestContentType.GetGenericTypeDefinition() == typeof(List<>) &&
            requestContentType.GetGenericArguments()[0] == typeof(KeyValuePair<string, string>))
            return new FormUrlEncodedContent(requestContent as List<KeyValuePair<string, string>>);


        var contentToAppendInRequest = new List<KeyValuePair<string, string>>();
        var properties = requestContentType.GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(requestContent, null);

            if (value is null)
                contentToAppendInRequest.Add(new KeyValuePair<string, string>(property.Name, string.Empty));
            else
                contentToAppendInRequest.Add(new KeyValuePair<string, string>(property.Name, value.ToString()));
        }

        return new FormUrlEncodedContent(contentToAppendInRequest);
    }

    private Uri GetUriToRequest(string uri)
    {
        if (string.IsNullOrEmpty(uri))
            throw new ArgumentNullException("fullUriOrPathUri");


        if (!Uri.TryCreate(uri, UriKind.Absolute, out var tryCreate))
            throw new Exception("fullUriOrPathUri invalid.");

        return tryCreate;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}