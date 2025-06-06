using System.Net;

namespace Wanadi.Common.Contracts.HttpScraping;

public sealed record HttpCookieResponse
{
    public HttpCookieResponse(Cookie cookie)
    {
        Path = cookie.Path;
        Value = cookie.Value;
        Name = cookie.Name;
        Expires = cookie.Expires;
        Domain = cookie.Domain;
    }

    /// <summary>
    /// Gets or sets the URI for which the System.Net.Cookie is valid.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// Gets or sets the URIs to which the System.Net.Cookie applies.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the System.Net.Cookie.Value for the System.Net.Cookie.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the name for the System.Net.Cookie.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time for the System.Net.Cookie as a DateTime.
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Full cookie for http request
    /// </summary>
    public string Cookie => $"{Name}={Value};";
}