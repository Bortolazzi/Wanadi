using System.Net;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Wanadi.Common.Contracts.RestApi;

/// <summary>
///     <para>
///         pt-BR: Classe padrão que encapsula os dados de resposta de uma requisição realizada pela RestApiClient.
///     </para>
///     <para>
///         en-US: Standard class that encapsulates the response data of a request made by RestApiClient.
///     </para>
/// </summary>
public class RestApiResponse
{
    public RestApiResponse() { }

    public RestApiResponse(RestApiResponse baseResponse)
    {
        Content = baseResponse.Content;
        MediaType = baseResponse.MediaType;
        StatusCode = baseResponse.StatusCode;
        IsSuccessStatusCode = baseResponse.IsSuccessStatusCode;
        Headers = baseResponse.Headers;
        Cookies = baseResponse.Cookies;
    }

    /// <summary>
    ///  <para>
    ///     pt-BR: Conteúdo retornado pela API
    ///  </para>
    ///  <para>
    ///     en-US: Content returned by the API
    ///  </para>
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: MediaType retornado pela API
    ///     </para>
    ///     <para>
    ///         en-US: MediaType returned by the API
    ///     </para>
    /// </summary>
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

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura do HttpResponseMessage e converte para o objeto.
    ///     </para
    ///     <para>
    ///         en-US: Reads the HttpResponseMessage and converts it to the object.
    ///     </para>
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <para>
    ///         pt-BR: HttpResponseMessage que será lido e convertido.
    ///     </para>
    ///     <para>
    ///         en-US: HttpResponseMessage that will be read and converted.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding que será utilizado na leitura do corpo da resposta.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding that will be used to read the response body.
    ///     </para>
    /// </param>
    /// <param name="cookieContainer">
    ///     <para>
    ///         pt-BR: CookieContainer para recuperar os cookies que a API pode vir a retornar.
    ///     </para>
    ///     <para>
    ///         en-US: CookieContainer to retrieve the cookies that the API may return.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto RestApiResponse contendo os dados da resposta da requisição.
    ///     </para>
    ///     <para>
    ///         en-US: RestApiResponse object containing the request response data.
    ///     </para>
    /// </returns>
    public static async Task<RestApiResponse> ReadResponseAsync(HttpResponseMessage httpResponseMessage, Encoding encoding, CookieContainer? cookieContainer)
    {
        var response = new RestApiResponse();

        response.StatusCode = httpResponseMessage.StatusCode;
        response.MediaType = httpResponseMessage.Content.Headers.ContentType?.MediaType;
        response.IsSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;
        response.Content = await httpResponseMessage.ReadContentToStringAsync(encoding);

        foreach (var header in httpResponseMessage.Headers)
            response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));

        if (cookieContainer != null && cookieContainer.Count > 0)
            response.Cookies.AddRange(cookieContainer.GetAllCookies().Select(t => new RestApiCookieResponse(t)).ToList());
        
        return response;
    }
}

/// <summary>
///     <para>
///         pt-BR: Classe customizada que encapsula os dados de resposta de uma requisição realizada pela RestApiClient e converte o Content em um objeto (Newtonsoft.Json).
///     </para>
///     <para>
///         en-US: Custom class that encapsulates the response data of a request made by RestApiClient and converts the Content into an object (Newtonsoft.Json).
///     </para>
/// </summary>
public class RestApiResponse<TResponse> : RestApiResponse where TResponse : class
{
    public RestApiResponse(RestApiResponse baseResponse) : base(baseResponse) { }

    /// <summary>
    ///     <para>
    ///         pt-BR: Objeto convertido através da propriedade Content utilizando Newtonsoft.
    ///     </para>
    ///     <para>
    ///         en-US: Object converted through the Content property using Newtonsoft.
    ///     </para>
    ///     <code>
    ///         <![CDATA[ Response = JsonConvert.DeserializeObject<TResponse>(Content); ]]>
    ///     </code>
    /// </summary>
    public TResponse? Response { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura do HttpResponseMessage e converte para o objeto.
    ///     </para
    ///     <para>
    ///         en-US: Reads the HttpResponseMessage and converts it to the object.
    ///     </para>
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <para>
    ///         pt-BR: HttpResponseMessage que será lido e convertido.
    ///     </para>
    ///     <para>
    ///         en-US: HttpResponseMessage that will be read and converted.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding que será utilizado na leitura do corpo da resposta.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding that will be used to read the response body.
    ///     </para>
    /// </param>
    /// <param name="cookieContainer">
    ///     <para>
    ///         pt-BR: CookieContainer para recuperar os cookies que a API pode vir a retornar.
    ///     </para>
    ///     <para>
    ///         en-US: CookieContainer to retrieve the cookies that the API may return.
    ///     </para>
    /// </param>
    /// <param name="ignoreResponseDeserializeError">
    ///     <para>
    ///         pt-BR: Caso esteja true e o Content gere uma exceção ao ser convertido pelo Newtonsoft, o erro é suprimido e retornado com o objeto Response nulo. Caso false, será lançada uma exceção.
    ///     </para>
    ///     <para>
    ///         en-US: If it is true and Content generates an exception when converted by Newtonsoft, the error is suppressed and returned with a null Response object. If false, an exception will be thrown.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto RestApiResponse contendo os dados da resposta da requisição.
    ///     </para>
    ///     <para>
    ///         en-US: RestApiResponse object containing the request response data.
    ///     </para>
    /// </returns>
    public static async Task<RestApiResponse<TResponse>> ReadResponseAsync(HttpResponseMessage httpResponseMessage, Encoding encoding, CookieContainer? cookieContainer, bool ignoreResponseDeserializeError)
    {
        var response = new RestApiResponse<TResponse>(await RestApiResponse.ReadResponseAsync(httpResponseMessage, encoding, cookieContainer));

        if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(response.Content))
        {
            try
            {
                response.Response = JsonConvert.DeserializeObject<TResponse>(response.Content);
            }
            catch
            {
                if (!ignoreResponseDeserializeError)
                    throw;
            }
        }

        return response;
    }
}

/// <summary>
///     <para>
///         pt-BR: Classe customizada que encapsula os dados de resposta de uma requisição realizada pela RestApiClient e converte o Content em um objeto, caso o StatusCode não seja sucesso, converte o conteúdo em um objeto TError a ser definido na chamada. (Conversão via Newtonsoft).
///     </para>
///     <para>
///         en-US: Custom class that encapsulates the response data of a request made by RestApiClient and converts the Content into an object. If the StatusCode is not successful, it converts the content into a TError object to be defined in the call. (Conversion via Newtonsoft).
///     </para>
/// </summary>
public class RestApiResponse<TResponse, TError> : RestApiResponse<TResponse> where TResponse : class where TError : class
{
    public RestApiResponse(RestApiResponse baseResponse) : base(baseResponse) { }

    /// <summary>
    ///     <para>
    ///         pt-BR: Caso a resposta da API venha com a propriedade IsSuccessStatusCode = false, o Content será convertido para esse objeto.
    ///     </para>
    ///     <para>
    ///         en-US: If the API response comes with the property IsSuccessStatusCode = false, the Content will be converted to this object.
    ///     </para>
    /// </summary>
    public TError? ErrorResponse { get; set; }

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura do HttpResponseMessage e converte para o objeto.
    ///     </para
    ///     <para>
    ///         en-US: Reads the HttpResponseMessage and converts it to the object.
    ///     </para>
    /// </summary>
    /// <param name="httpResponseMessage">
    ///     <para>
    ///         pt-BR: HttpResponseMessage que será lido e convertido.
    ///     </para>
    ///     <para>
    ///         en-US: HttpResponseMessage that will be read and converted.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding que será utilizado na leitura do corpo da resposta.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding that will be used to read the response body.
    ///     </para>
    /// </param>
    /// <param name="cookieContainer">
    ///     <para>
    ///         pt-BR: CookieContainer para recuperar os cookies que a API pode vir a retornar.
    ///     </para>
    ///     <para>
    ///         en-US: CookieContainer to retrieve the cookies that the API may return.
    ///     </para>
    /// </param>
    /// <param name="ignoreResponseDeserializeError">
    ///     <para>
    ///         pt-BR: Caso esteja true e o Content gere uma exceção ao ser convertido pelo Newtonsoft, o erro é suprimido e retornado com o objeto Response nulo. Caso false, será lançada uma exceção.
    ///     </para>
    ///     <para>
    ///         en-US: If it is true and Content generates an exception when converted by Newtonsoft, the error is suppressed and returned with a null Response object. If false, an exception will be thrown.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Objeto RestApiResponse contendo os dados da resposta da requisição.
    ///     </para>
    ///     <para>
    ///         en-US: RestApiResponse object containing the request response data.
    ///     </para>
    /// </returns>
    public static new async Task<RestApiResponse<TResponse, TError>> ReadResponseAsync(HttpResponseMessage httpResponseMessage, Encoding encoding, CookieContainer? cookieContainer, bool ignoreResponseDeserializeError)
    {
        var response = new RestApiResponse<TResponse, TError>(await RestApiResponse.ReadResponseAsync(httpResponseMessage, encoding, cookieContainer));

        if (!string.IsNullOrEmpty(response.Content))
        {
            try
            {
                if (response.IsSuccessStatusCode)
                    response.Response = JsonConvert.DeserializeObject<TResponse>(response.Content);
                else
                    response.ErrorResponse = JsonConvert.DeserializeObject<TError>(response.Content);
            }
            catch
            {
                if (!ignoreResponseDeserializeError)
                    throw;
            }
        }

        return response;
    }
}