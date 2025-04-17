namespace DergiMBackend.Models;

public class ApiSettings
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public TimeSpan TokenExpiration { get; set; }
    public string SecretKey { get; set; } = default!;

}