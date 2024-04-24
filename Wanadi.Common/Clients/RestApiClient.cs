using System.Net;
using Newtonsoft.Json;
using Wanadi.Common.Contracts.RestApi;

namespace Wanadi.Common.Clients;

/// <summary>
///     <para>
///         pt-BR: Classe dinâmica e abstrata que encapsula as requisições HTTP - GET, POST, PUT, PATCH e DELETE.
///     </para>
///     <para>
///         en-US: Dynamic and abstract class that encapsulates GET, POST, PUT, PATCH and DELETE HTTP requests.
///     </para>
/// </summary>
public abstract class RestApiClient : IDisposable
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Encoding a ser utilizado na escrita e leitura dos corpos das mensagens.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding to be used when writing and reading message bodies.
    ///     </para>
    /// </summary>
    protected Encoding ApiEncoding { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Indicador de auto redirecionamento de requisições. (HttpStatusCode 302)
    ///     </para>
    ///     <para>
    ///         en-US: Auto redirection indicator for requests. (HttpStatusCode 302)
    ///     </para>
    /// </summary>
    protected bool AllowAutoRedirect { get; set; } = false;

    /// <summary>
    ///     <para>
    ///         pt-BR: Indicador de byPass para certificados SSL fora de vigência ou inválidos.
    ///     </para>
    ///     <para>
    ///         en-US: ByPass indicator for out-of-date or invalid SSL certificates.
    ///     </para>
    /// </summary>
    protected bool AllowByPassCertificateCheck { get; set; } = false;

    /// <summary>
    ///     <para>
    ///         pt-BR: Indicador de ignorar erros de desserialização do Content para TResponse ou TError.
    ///     </para>
    ///     <para>
    ///         Se True: Quando houver um erro na desserialização do TResponse ou do TError o erro será suprimido. Se False: Exceção será lançada.
    ///     </para>
    ///     <para>
    ///         en-US: Indicator for ignoring Content deserialization errors for TResponse or TError.
    ///     </para>
    ///     <para>
    ///         If True: When there is an error in the deserialization of TResponse or TError, the error will be suppressed. If False: Exception will be thrown.
    ///     </para>
    /// </summary>
    protected bool IgnoreResponseDeserializeError { get; set; } = false;

    protected HttpClient _httpClient;
    protected HttpClientHandler _httpClientHandler;

    private string MediaType => "application/json";

    private Uri? BaseUri { get; set; }
    protected CookieContainer? ContainerCookie { get; set; }
    private WebProxy? Proxy { get; set; }

    public RestApiClient() : this(Encoding.UTF8) { }

    public RestApiClient(Encoding apiEncoding)
        => ApiEncoding = apiEncoding;

    /// <summary>
    ///     <para>
    ///         pt-BR: Seta uma BaseAddress para as requisições, quando informada as demais chamadas podem utilizar apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: Set a BaseAddress for requests, when informed, other calls can only use the relative URL.
    ///     </para>
    /// </summary>
    /// <param name="baseUri">
    ///     <para>
    ///         pt-BR: BaseAddress a ser utilizado. 
    ///     </para>
    ///     <para>
    ///         en-US: BaseAddress to be used.
    ///     </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <para>
    ///         pt-BR: baseUri se informada deve ser diferente de branco/nulo.
    ///     </para>
    ///     <para>
    ///         pt-BR: baseUri if informed must be different from empty/null.
    ///     </para>
    /// </exception>
    /// <exception cref="Exception">
    ///     <para>
    ///         pt-BR: baseUri se informada deve ser uma Uri válida.
    ///     </para>
    ///     <para>
    ///         pt-BR: baseUri if provided must be a valid Uri.
    ///     </para>
    /// </exception>
    protected void SetBaseUri(string baseUri)
    {
        if (string.IsNullOrEmpty(baseUri))
            throw new ArgumentNullException("baseUri");

        if (!Uri.TryCreate(baseUri, UriKind.Absolute, out var tryCreate))
            throw new Exception("Base Uri invalid.");

        BaseUri = tryCreate;
    }

    protected void SetWebProxy(string ipAddress, int port, string user, string password)
    {
        var proxy = new WebProxy($"{ipAddress}:{port}");
        proxy.Credentials = new NetworkCredential(user, password);
        SetWebProxy(proxy);
    }

    protected void SetWebProxy(WebProxy proxy)
        => Proxy = proxy;

    #region [GET]

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição GET na URL informada.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a GET request on the specified URL.
    ///     </para>
    /// </summary>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse> GetAsync(string fullUriOrPathUri, Dictionary<string, string>? headers = null)
        => await SendAndParseResponseAsync(fullUriOrPathUri, HttpMethod.Get, null, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição GET na URL informada. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a GET request on the specified URL. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content in TResponse.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse>> GetAsync<TResponse>(string fullUriOrPathUri, Dictionary<string, string>? headers = null) where TResponse : class
        => await SendAndParseResponseAsync<TResponse>(fullUriOrPathUri, HttpMethod.Get, null, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição GET na URL informada. Desserializa o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a GET request on the specified URL. Deserializes Content into TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <typeparam name="TError">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content. Quando a requisição obtém status diferente de sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content. When the request obtains a status other than success.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse quando sucesso e desserializando o Content em TError quando falha.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content into TResponse when successful and deserializing Content into TError when failed.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse, TError>> GetAsync<TResponse, TError>(string fullUriOrPathUri, Dictionary<string, string>? headers = null) where TResponse : class where TError : class
        => await SendAndParseResponseAsync<TResponse, TError>(fullUriOrPathUri, HttpMethod.Get, null, headers);

    #endregion [GET]

    #region [POST]

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição POST na URL informada anexando o corpo quando informado.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a POST request on the specified URL, appending the body when informed.
    ///     </para>
    /// </summary>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse> PostAsync(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null)
        => await SendAndParseResponseAsync(fullUriOrPathUri, HttpMethod.Post, body, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição POST na URL informada anexando o corpo quando informado. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a POST request on the specified URL, appending the body when informed. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content in TResponse.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse>> PostAsync<TResponse>(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null) where TResponse : class
        => await SendAndParseResponseAsync<TResponse>(fullUriOrPathUri, HttpMethod.Post, body, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição POST na URL informada anexando o corpo quando informado. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a POST request on the specified URL, appending the body when informed. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <typeparam name="TError">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content. Quando a requisição obtém status diferente de sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content. When the request obtains a status other than success.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse quando sucesso e desserializando o Content em TError quando falha.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content into TResponse when successful and deserializing Content into TError when failed.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse, TError>> PostAsync<TResponse, TError>(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null) where TResponse : class where TError : class
        => await SendAndParseResponseAsync<TResponse, TError>(fullUriOrPathUri, HttpMethod.Post, body, headers);

    #endregion [POST]

    #region [DELETE]

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição DELETE na URL informada anexando o corpo quando informado.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a DELETE request on the specified URL, appending the body when informed.
    ///     </para>
    /// </summary>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse> DeleteAsync(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null)
        => await SendAndParseResponseAsync(fullUriOrPathUri, HttpMethod.Delete, body, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição DELETE na URL informada anexando o corpo quando informado. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a DELETE request on the specified URL, appending the body when informed. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content in TResponse.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse>> DeleteAsync<TResponse>(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null) where TResponse : class
        => await SendAndParseResponseAsync<TResponse>(fullUriOrPathUri, HttpMethod.Delete, body, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição DELETE na URL informada anexando o corpo quando informado. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a DELETE request on the specified URL, appending the body when informed. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <typeparam name="TError">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content. Quando a requisição obtém status diferente de sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content. When the request obtains a status other than success.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse quando sucesso e desserializando o Content em TError quando falha.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content into TResponse when successful and deserializing Content into TError when failed.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse, TError>> DeleteAsync<TResponse, TError>(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null) where TResponse : class where TError : class
        => await SendAndParseResponseAsync<TResponse, TError>(fullUriOrPathUri, HttpMethod.Delete, body, headers);

    #endregion [DELETE]

    #region [PATCH]

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição PATCH na URL informada anexando o corpo quando informado.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a PATCH request on the specified URL, appending the body when informed.
    ///     </para>
    /// </summary>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse> PatchAsync(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null)
        => await SendAndParseResponseAsync(fullUriOrPathUri, HttpMethod.Patch, body, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição PATCH na URL informada anexando o corpo quando informado. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a PATCH request on the specified URL, appending the body when informed. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content in TResponse.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse>> PatchAsync<TResponse>(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null) where TResponse : class
        => await SendAndParseResponseAsync<TResponse>(fullUriOrPathUri, HttpMethod.Patch, body, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição PATCH na URL informada anexando o corpo quando informado. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a PATCH request on the specified URL, appending the body when informed. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <typeparam name="TError">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content. Quando a requisição obtém status diferente de sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content. When the request obtains a status other than success.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse quando sucesso e desserializando o Content em TError quando falha.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content into TResponse when successful and deserializing Content into TError when failed.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse, TError>> PatchAsync<TResponse, TError>(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null) where TResponse : class where TError : class
        => await SendAndParseResponseAsync<TResponse, TError>(fullUriOrPathUri, HttpMethod.Patch, body, headers);

    #endregion [PATCH]

    #region [PUT]

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição PUT na URL informada anexando o corpo quando informado.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a PUT request on the specified URL, appending the body when informed.
    ///     </para>
    /// </summary>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse> PutAsync(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null)
        => await SendAndParseResponseAsync(fullUriOrPathUri, HttpMethod.Put, body, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição PUT na URL informada anexando o corpo quando informado. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a PUT request on the specified URL, appending the body when informed. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content in TResponse.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse>> PutAsync<TResponse>(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null) where TResponse : class
        => await SendAndParseResponseAsync<TResponse>(fullUriOrPathUri, HttpMethod.Put, body, headers);

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza uma requisição PATCH na URL informada anexando o corpo quando informado. Converte o Content em objeto TResponse.
    ///     </para>
    ///     <para>
    ///         en-US: Performs a PATCH request on the specified URL, appending the body when informed. Converts Content to TResponse object.
    ///     </para>
    /// </summary>
    /// <typeparam name="TResponse">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content quando a requisição obtém sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content when the request is successful.
    ///     </para>
    /// </typeparam>
    /// <typeparam name="TError">
    ///     <para>
    ///         pt-BR: Tipo do objeto a ser desserializado o valor recebido através do Content. Quando a requisição obtém status diferente de sucesso.
    ///     </para>
    ///     <para>
    ///         en-US: Type of object to be deserialized the value received through Content. When the request obtains a status other than success.
    ///     </para>
    /// </typeparam>
    /// <param name="fullUriOrPathUri">
    ///     <para>
    ///         pt-BR: URL para realização da requisição. Caso tenha informado o BaseAddres via SetBaseUri, informe apenas o relative URL.
    ///     </para>
    ///     <para>
    ///         en-US: URL to make the request. If you entered BaseAddres via SetBaseUri, only enter the relative URL.
    ///     </para>
    /// </param>
    /// <param name="body">
    ///     <para>
    ///         pt-BR: Corpo a ser anexado na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Body to be attached to the request.
    ///     </para>
    /// </param>
    /// <param name="headers">
    ///     <para>
    ///         pt-BR: Cabeçalhos a serem anexados na requisição.
    ///     </para>
    ///     <para>
    ///         en-US: Headers to be attached to the request.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto contendo as informações da resposta da requisição. Desserializando o Content em TResponse quando sucesso e desserializando o Content em TError quando falha.
    ///     </para>
    ///     <para>
    ///         en-US: Object containing the request response information. Deserializing Content into TResponse when successful and deserializing Content into TError when failed.
    ///     </para>
    /// </returns>
    protected virtual async Task<RestApiResponse<TResponse, TError>> PutAsync<TResponse, TError>(string fullUriOrPathUri, object? body, Dictionary<string, string>? headers = null) where TResponse : class where TError : class
        => await SendAndParseResponseAsync<TResponse, TError>(fullUriOrPathUri, HttpMethod.Put, body, headers);

    #endregion [PUT]

    private async Task<RestApiResponse> SendAndParseResponseAsync(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? body,
        Dictionary<string, string>? headers)
    {
        using (var httpResponseMessage = await SendAsync(fullUriOrPathUri, httpMethod, body, headers))
            return await RestApiResponse.ReadResponseAsync(httpResponseMessage, ApiEncoding, ContainerCookie);
    }

    private async Task<RestApiResponse<TResponse>> SendAndParseResponseAsync<TResponse>(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? body,
        Dictionary<string, string>? headers) where TResponse : class
    {
        using (var httpResponseMessage = await SendAsync(fullUriOrPathUri, httpMethod, body, headers))
            return await RestApiResponse<TResponse>.ReadResponseAsync(httpResponseMessage, ApiEncoding, ContainerCookie, IgnoreResponseDeserializeError);
    }

    private async Task<RestApiResponse<TResponse, TError>> SendAndParseResponseAsync<TResponse, TError>(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? body,
        Dictionary<string, string>? headers) where TResponse : class where TError : class
    {
        using (var httpResponseMessage = await SendAsync(fullUriOrPathUri, httpMethod, body, headers))
            return await RestApiResponse<TResponse, TError>.ReadResponseAsync(httpResponseMessage, ApiEncoding, ContainerCookie, IgnoreResponseDeserializeError);
    }

    private async Task<HttpResponseMessage> SendAsync(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? body,
        Dictionary<string, string>? headers)
    {
        var _client = InstanceHttpClient();
        using (var _message = BuildRequestMessage(fullUriOrPathUri, httpMethod, body, headers))
            return await _client.SendAsync(_message);
    }

    private HttpRequestMessage BuildRequestMessage(
        string fullUriOrPathUri,
        HttpMethod httpMethod,
        object? body,
        Dictionary<string, string>? headers)
    {
        var response = new HttpRequestMessage(httpMethod, GetUriToRequest(fullUriOrPathUri));

        if (headers != null && headers.Count > 0)
        {
            foreach (var header in headers)
                response.Headers.Add(header.Key, header.Value);
        }

        if (httpMethod == HttpMethod.Get || body == null)
            return response;

        response.Content = new StringContent(JsonConvert.SerializeObject(body), ApiEncoding, MediaType);
        return response;
    }

    protected HttpClient InstanceHttpClient()
    {
        if (_httpClient is not null)
            return _httpClient;

        ContainerCookie = new CookieContainer();

        _httpClientHandler = new HttpClientHandler();
        _httpClientHandler.CookieContainer = ContainerCookie;
        _httpClientHandler.AllowAutoRedirect = AllowAutoRedirect;

        if (AllowByPassCertificateCheck)
            _httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

        if (Proxy != null)
            _httpClientHandler.Proxy = Proxy;

        _httpClient = new HttpClient(_httpClientHandler);
        return _httpClient;
    }

    private Uri GetUriToRequest(string uri)
    {
        if (string.IsNullOrEmpty(uri))
            throw new ArgumentNullException("fullUriOrPathUri");

        if (BaseUri == null)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var tryCreate))
                throw new Exception("fullUriOrPathUri invalid.");

            return tryCreate;
        }

        var baseUri = BaseUri.AbsoluteUri.TrimEnd('/');
        var partialUri = uri.TrimStart('/');

        if (!Uri.TryCreate($"{baseUri}/{partialUri}", UriKind.Absolute, out var tryUri))
            throw new Exception("Invalid uri.");

        return tryUri;
    }

    public void Dispose()
    {
        _httpClientHandler?.Dispose();
        _httpClient?.Dispose();
    }
}