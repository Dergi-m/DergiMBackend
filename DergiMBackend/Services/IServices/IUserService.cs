using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;

namespace DergiMBackend.Services.IServices
{
	public interface IUserService
	{
		Task<IEnumerable<UserDto>> GetUsersAsync(string? organisationUniqueName = null);
		Task<UserDto> GetUserAsync(string username);
		bool IsUniqueUser(string username);
		Task<SessionDto> Login(LoginRequestDto loginRequestDto);
		Task<UserDto> Register(RegistrationRequestDto registrationRequestDto);
		Task<UserDto> AssignUserToRole(string userName, string roleId);
		Task<bool> AssignUserToOrganisation(ApplicationUser user);
	}
}
