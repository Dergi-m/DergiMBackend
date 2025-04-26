using DergiMBackend.Models.Dtos;

namespace DergiMBackend.Services.IServices
{
    public interface IUserService
    {
        Task<SessionDto> LoginAsync(LoginRequestDto request);
        Task<SessionDto> RegisterAsync(RegistrationRequestDto request);
        Task<IEnumerable<UserDto>> GetUsersAsync(string? organisationUniqueName = null);
        Task<UserDto> GetUserAsync(string username);
    }
}
