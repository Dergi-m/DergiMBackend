using DergiMBackend.Models.Dtos;

namespace DergiMBackend.Services.IServices;

public interface IAuthService
{
    Task<TokenResponseDto?> GetAccessTokenAsync(TokenRequestDto request);
}

