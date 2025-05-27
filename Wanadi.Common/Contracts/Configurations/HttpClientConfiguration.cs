namespace Wanadi.Common.Contracts.Configurations;

public sealed record HttpClientConfiguration
{
    public required string Name { get; set; }
    public int? TimeoutSeconds { get; set; }
    public bool AllowAutoRedirect { get; set; } = false;
    public bool AllowByPassCertificateCheck { get; set; } = true;
    public bool UseCookies { get; set; } = true;
    public string? ProxyAddress { get; set; }
    public string? ProxyUser { get; set; }
    public string? ProxyPassword { get; set; }
}