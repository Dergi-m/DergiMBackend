
namespace DergiMBackend.Models.Dtos
{
	public class UserDto
	{
		public required string Id { get; set; } = default!;
		public string? Name { get; set; }
		public required string UserName { get; set; }
		public UserRole? Role { get; set; }
		public required string OrganisationUniqueName { get; set; }

    }
}
