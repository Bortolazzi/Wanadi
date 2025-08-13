using System.Web;

namespace Wanadi.Common.Extensions;

public static class HttpResponseMessageExtensions
{
    private static readonly Dictionary<string, string> MimeToExtension = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Texto
        { "text/plain", ".txt" },
        { "text/html", ".html" },
        { "text/css", ".css" },
        { "text/csv", ".csv" },
        { "text/xml", ".xml" },
        { "application/xml", ".xml" },
        { "application/xhtml+xml", ".xhtml" },
        { "application/json", ".json" },
        { "application/javascript", ".js" },

        // Imagens
        { "image/jpeg", ".jpg" },
        { "image/png", ".png" },
        { "image/gif", ".gif" },
        { "image/bmp", ".bmp" },
        { "image/webp", ".webp" },
        { "image/tiff", ".tiff" },
        { "image/svg+xml", ".svg" },
        { "image/x-icon", ".ico" },

        // Áudio
        { "audio/mpeg", ".mp3" },
        { "audio/wav", ".wav" },
        { "audio/ogg", ".ogg" },
        { "audio/webm", ".weba" },
        { "audio/flac", ".flac" },

        // Vídeo
        { "video/mp4", ".mp4" },
        { "video/x-msvideo", ".avi" },
        { "video/x-ms-wmv", ".wmv" },
        { "video/mpeg", ".mpeg" },
        { "video/webm", ".webm" },
        { "video/ogg", ".ogv" },
        { "application/vnd.apple.mpegurl", ".m3u8" },
        { "video/MP2T", ".ts" },

        // Documentos
        { "application/pdf", ".pdf" },
        { "application/msword", ".doc" },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx" },
        { "application/vnd.ms-excel", ".xls" },
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx" },
        { "application/vnd.ms-powerpoint", ".ppt" },
        { "application/vnd.openxmlformats-officedocument.presentationml.presentation", ".pptx" },

        // Compactação
        { "application/zip", ".zip" },
        { "application/x-rar-compressed", ".rar" },
        { "application/x-7z-compressed", ".7z" },
        { "application/gzip", ".gz" },
        { "application/x-tar", ".tar" },

        // Fontes
        { "font/ttf", ".ttf" },
        { "font/otf", ".otf" },
        { "application/font-woff", ".woff" },
        { "application/font-woff2", ".woff2" },

        // MIME genérico
        { "application/octet-stream", ".bin" },
        { "application/x-binary", ".bin" },

        // Outros úteis
        { "application/x-shockwave-flash", ".swf" },
        { "application/epub+zip", ".epub" },
        { "application/x-msdownload", ".exe" },
        { "application/x-iso9660-image", ".iso" }
    };

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
    public static async Task<string> ReadContentToStringAsync(this HttpResponseMessage responseMessage, Encoding encoding, CancellationToken cancellationToken)
    {
        using var stream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, encoding);
        return HttpUtility.HtmlDecode(await reader.ReadToEndAsync());
    }

    public static string GetExtensionFromResponse(this HttpResponseMessage response)
    {
        // Tentativa de extrair do header Content-Disposition
        var contentDisposition = response.Content.Headers.ContentDisposition;
        if (contentDisposition != null && !string.IsNullOrEmpty(contentDisposition.FileName))
        {
            var extension = Path.GetExtension(contentDisposition.FileName);
            if (extension is { Length: > 0 })
                return extension.Replace("\"", string.Empty);
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;

        if (!string.IsNullOrWhiteSpace(mediaType) && MimeToExtension.TryGetValue(mediaType, out var ext))
            return ext;

        return ".dat";
    }
}