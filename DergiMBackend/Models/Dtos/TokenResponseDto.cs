namespace DergiMBackend.Models.Dtos;
public class TokenResponseDto
{
    public string? AccessToken { get; set; }
    public TimeSpan ExpiresIn { get; set; }
}