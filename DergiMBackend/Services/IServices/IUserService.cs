using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;

namespace DergiMBackend.Services.IServices
{
    public interface IUserService
    {
        Task<SessionDto> LoginAsync(LoginRequestDto loginRequestDto);
        Task<SessionDto> RegisterAsync(RegistrationRequestDto registrationRequestDto);
        Task<UserDto> GetUserAsync(string username);
        Task<UserDto> GetUserByIdAsync(string uid);
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<bool> IsUserUniqueAsync(string username, string email);
        Task<ApplicationUser> GetUserEntityByIdAsync(string id);

    }
}
