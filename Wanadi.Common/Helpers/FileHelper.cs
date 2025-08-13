using System.Security.Cryptography;
using Microsoft.ProgramSynthesis.Detection.Encoding;
using UtfUnknown;

namespace Wanadi.Common.Helpers;

public static class FileHelper
{
    /// <summary>
    ///     <para>
    ///         pt-BR: Calcula o hash MD5 do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Calculates the MD5 hash from file.
    ///     </para>
    /// </summary>
    /// <param name="fileName">
    ///     <para>
    ///         pt-BR: Caminho do arquivo a ser calculado o hash MD5.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file to calculate the MD5 hash.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Hash MD5 do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: MD5 hash of the file.
    ///     </para>
    /// </returns>
    public static string HashMD5(string fileName)
    {
        using (MD5 md5Generate = MD5.Create())
        using (FileStream fileStream = File.OpenRead(fileName))
            return Convert.ToBase64String(md5Generate.ComputeHash(fileStream));
    }

    #region [Read all text]

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura de um arquivo em texto identificando o Encoding automaticamente.
    ///     </para>
    ///     <para>
    ///         en-US: Reads a text file by automatically identifying the Encoding.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Arquivo lido em texto.
    ///     </para>
    ///     <para>
    ///         en-US: File Read in Text.
    ///     </para>
    /// </returns>
    public static async Task<string> ReadAsync(string filePath)
       => await FileHelper.ReadAsync(filePath, await FileHelper.DetectEncodingAsync(filePath));

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura de um arquivo em texto utilizando o Encoding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Reads a text file using the specified Encoding.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="encodingFile">
    ///     <para>
    ///         pt-BR: Encoding para leitura.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding for reading.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Arquivo lido em texto.
    ///     </para>
    ///     <para>
    ///         en-US: File Read in Text.
    ///     </para>
    /// </returns>
    public static async Task<string> ReadAsync(string filePath, Encoding encodingFile)
    {
        using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var streamReader = new StreamReader(fileStream, encodingFile))
            return await streamReader.ReadToEndAsync();
    }

    #endregion [Read all text]

    #region [Read all lines]

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura de um arquivo em linhas identificando o Encoding automaticamente.
    ///     </para>
    ///     <para>
    ///         en-US: Reads a file in lines, automatically identifying the Encoding.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Arquivo lido em linhas.
    ///     </para>
    ///     <para>
    ///         en-US: File Read in lines.
    ///     </para>
    /// </returns>
    public static async Task<List<string>> ReadLinesAsync(string filePath)
        => await FileHelper.ReadLinesAsync(filePath, await FileHelper.DetectEncodingAsync(filePath));

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura de um arquivo em linhas utilizando o Encoding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Reads a file in lines using the specified Encoding.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="encodingFile">
    ///     <para>
    ///         pt-BR: Encoding para leitura.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding for reading.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Arquivo lido em linhas.
    ///     </para>
    ///     <para>
    ///         en-US: File Read in lines.
    ///     </para>
    /// </returns>
    public static async Task<List<string>> ReadLinesAsync(string filePath, Encoding encodingFile)
    {
        var response = new List<string>();

        using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var streamReader = new StreamReader(fileStream, encodingFile))
        {
            string? _line = null;
            while ((_line = await streamReader.ReadLineAsync()) != null)
                response.Add(_line);
        }

        return response;
    }

    #endregion [Read all lines]

    #region [Create file]

    /// <summary>
    ///     <para>
    ///         pt-BR: Cria um arquivo escrevendo o conteúdo informado. Utilizando o Encoding UTF8
    ///     </para>
    ///     <para>
    ///         en-US: Creates a file writing the informed content. Using UTF8 Encoding
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="content">
    ///     <para>
    ///         pt-BR: Conteúdo a ser escrito.
    ///     </para>
    ///     <para>
    ///         en-US: Content to be written.
    ///     </para>
    /// </param>
    public static async Task CreateAsync(string filePath, string content)
        => await FileHelper.CreateAsync(filePath, content, Encoding.UTF8);

    /// <summary>
    ///     <para>
    ///         pt-BR: Cria um arquivo escrevendo o conteúdo informado. Utilizando o Encoding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Creates a file writing the informed content. Using the informed Encoding
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="content">
    ///     <para>
    ///         pt-BR: Conteúdo a ser escrito.
    ///     </para>
    ///     <para>
    ///         en-US: Content to be written.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding a ser utilizado na escrita do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding to be used when writing the file.
    ///     </para>
    /// </param>
    public static async Task CreateAsync(string filePath, string content, Encoding encoding)
    {
        using (var _streamWriter = new StreamWriter(filePath, false, encoding))
        {
            _streamWriter.AutoFlush = true;
            await _streamWriter.WriteAsync(content);
            await _streamWriter.FlushAsync();
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Cria um arquivo escrevendo o conteúdo informado. Utilizando o Encoding UTF8
    ///     </para>
    ///     <para>
    ///         en-US: Creates a file writing the informed content. Using UTF8 Encoding
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="lines">
    ///     <para>
    ///         pt-BR: Conteúdo a ser escrito.
    ///     </para>
    ///     <para>
    ///         en-US: Content to be written.
    ///     </para>
    /// </param>
    public static async Task CreateAsync(string filePath, List<string> lines)
        => await FileHelper.CreateAsync(filePath, lines, Encoding.UTF8);

    /// <summary>
    ///     <para>
    ///         pt-BR: Cria um arquivo escrevendo o conteúdo informado. Utilizando o Encoding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Creates a file writing the informed content. Using the informed Encoding
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="lines">
    ///     <para>
    ///         pt-BR: Conteúdo a ser escrito.
    ///     </para>
    ///     <para>
    ///         en-US: Content to be written.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding a ser utilizado na escrita do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding to be used when writing the file.
    ///     </para>
    /// </param>
    public static async Task CreateAsync(string filePath, List<string> lines, Encoding encoding)
    {
        using (var _streamWriter = new StreamWriter(filePath, false, encoding))
        {
            _streamWriter.AutoFlush = true;

            foreach (string line in lines)
                await _streamWriter.WriteLineAsync(line);

            await _streamWriter.FlushAsync();
        }
    }

    #endregion [Create file]

    #region [Append text]

    /// <summary>
    ///     <para>
    ///         pt-BR: Acrescenta conteúdo a um arquivo utilizando UTF8 como encoding.
    ///     </para>
    ///     <para>
    ///         en-US: Append content to a file using UTF8 as encoding.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="content">
    ///     <para>
    ///         pt-BR: Conteúdo a ser escrito.
    ///     </para>
    ///     <para>
    ///         en-US: Content to be written.
    ///     </para>
    /// </param>
    public static async Task AppendTextAsync(string filePath, string content)
        => await FileHelper.AppendTextAsync(filePath, content, Encoding.UTF8);

    /// <summary>
    ///     <para>
    ///         pt-BR: Acrescenta conteúdo a um arquivo utilizando o encoding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Append content to a file using the specified encoding
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="content">
    ///     <para>
    ///         pt-BR: Conteúdo a ser escrito.
    ///     </para>
    ///     <para>
    ///         en-US: Content to be written.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding a ser utilizado na escrita do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding to be used when writing the file.
    ///     </para>
    /// </param>
    public static async Task AppendTextAsync(string filePath, string content, Encoding encoding)
    {
        using (var _streamWriter = new StreamWriter(filePath, true, encoding))
        {
            _streamWriter.AutoFlush = true;
            await _streamWriter.WriteLineAsync(content);
            await _streamWriter.FlushAsync();
        }
    }

    /// <summary>
    ///     <para>
    ///         pt-BR: Acrescenta conteúdo a um arquivo utilizando UTF8 como encoding.
    ///     </para>
    ///     <para>
    ///         en-US: Append content to a file using UTF8 as encoding.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="lines">
    ///     <para>
    ///         pt-BR: Conteúdo a ser escrito.
    ///     </para>
    ///     <para>
    ///         en-US: Content to be written.
    ///     </para>
    /// </param>
    public static async Task AppendTextAsync(string filePath, List<string> lines)
        => await FileHelper.AppendTextAsync(filePath, lines, Encoding.UTF8);

    /// <summary>
    ///     <para>
    ///         pt-BR: Acrescenta conteúdo a um arquivo utilizando o encoding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Append content to a file using the specified encoding
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="lines">
    ///     <para>
    ///         pt-BR: Conteúdo a ser escrito.
    ///     </para>
    ///     <para>
    ///         en-US: Content to be written.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding a ser utilizado na escrita do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding to be used when writing the file.
    ///     </para>
    /// </param>
    public static async Task AppendTextAsync(string filePath, List<string> lines, Encoding encoding)
    {
        using (StreamWriter _streamWriter = new StreamWriter(filePath, true, encoding))
        {
            _streamWriter.AutoFlush = true;

            foreach (string linha in lines)
                await _streamWriter.WriteLineAsync(linha);

            await _streamWriter.FlushAsync();
        }
    }

    #endregion [Append text]

    #region [Read first line]

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura da primeira linha de um arquivo identificando o Encoding automaticamente.
    ///     </para>
    ///     <para>
    ///         en-US: Reads the first line of a file, automatically identifying the Encoding.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Retorna a primeira linha encontrada no arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Returns the first line found in the file.
    ///     </para>
    /// </returns>
    public static async Task<string?> ReadFirstLineAsync(string filePath)
        => await ReadFirstLineAsync(filePath, await FileHelper.DetectEncodingAsync(filePath));

    /// <summary>
    ///     <para>
    ///         pt-BR: Realiza a leitura da primeira linha de um arquivo com o Enconding informado.
    ///     </para>
    ///     <para>
    ///         en-US: Reads the first line of a file with the specified Enconding.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <param name="encoding">
    ///     <para>
    ///         pt-BR: Encoding a ser utilizado na escrita do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding to be used when writing the file.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Retorna a primeira linha encontrada no arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Returns the first line found in the file.
    ///     </para>
    /// </returns>
    public static async Task<string?> ReadFirstLineAsync(string filePath, Encoding encoding)
    {
        using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (StreamReader streamReader = new StreamReader(fileStream, encoding))
        {
            return await streamReader.ReadLineAsync();
        }
    }

    #endregion [Read first line]

    /// <summary>
    ///     <para>
    ///         pt-BR: Identifica o encoding que foi utilizado na escrita do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Identifies the encoding that was used to write the file.
    ///     </para>
    /// </summary>
    /// <param name="filePath">
    ///     <para>
    ///         pt-BR: Caminho do arquivo.
    ///     </para>
    ///     <para>
    ///         en-US: Path of the file.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para>
    ///         pt-BR: Encoding identificado.
    ///     </para>
    ///     <para>
    ///         en-US: Encoding identified.
    ///     </para>
    /// </returns>
    public static async Task<Encoding> DetectEncodingAsync(string filePath)
    {
        try
        {
            var utfResult = await CharsetDetector.DetectFromFileAsync(filePath);
            if (utfResult is { Detected: not null })
            {
                if (utfResult.Detected is { Encoding: not null })
                    return utfResult.Detected.Encoding;
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (stream.CanSeek)
                {
                    // Read from the beginning if possible
                    stream.Seek(0, SeekOrigin.Begin);
                }

                var encodingType = EncodingIdentifier.IdentifyEncoding(stream);
                var encodingDotNetName = EncodingTypeUtils.GetDotNetName(encodingType);

                if (!string.IsNullOrEmpty(encodingDotNetName))
                {
                    return Encoding.GetEncoding(encodingDotNetName);
                }
            }
        }
        catch
        {
            return Encoding.UTF8;
        }

        return Encoding.UTF8;
    }
}