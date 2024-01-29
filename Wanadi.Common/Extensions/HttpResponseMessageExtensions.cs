using System.Web;

namespace Wanadi.Common.Extensions;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura do conteúdo de uma resposta utilizando o encoding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Reads the content of a response using the informed encoding.
    ///     </para>
    /// </summary>
    /// <param name="responseMessage">
    ///     <para>
    ///         pt-BR: Mensagem a ter o conteúdo lido.
    ///     </para>
    ///     <para>
    ///         en-US: Message to have the content read.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding a ser utilizado na leitura.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding to be used in reading.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Conteúdo da mensagem lido em texto utilizando o encoding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Message content read in text using the informed encoding.
    ///     </para>
    /// </returns>
    public static async Task<string> ReadContentToStringAsync(this HttpResponseMessage responseMessage, Encoding encoding)
    {
        var byteArray = await responseMessage.Content.ReadAsByteArrayAsync();
        var response = encoding.GetString(byteArray, 0, byteArray.Length);
        return HttpUtility.HtmlDecode(response);
    }
}