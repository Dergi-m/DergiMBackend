using DergiMBackend.Models.Dtos;

namespace DergiMBackend.Services.IServices
{
	public interface IUserService
	{
		Task<IEnumerable<UserDto>> GetUsersAsync(int? organisationId = null);
		Task<UserDto> GetUserAsync(string username);
		bool IsUniqueUser(string username);
		Task<TokenDto> Login(LoginRequestDto loginRequestDto);
		Task<UserDto> Register(RegistrationRequestDto registrationRequestDto);
		Task<UserDto> AssignUserToRole(RegistrationRequestDto registrationRequestDto);
	}
}
