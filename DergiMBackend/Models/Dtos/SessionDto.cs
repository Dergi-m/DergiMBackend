
namespace DergiMBackend.Models.Dtos
{
	public class SessionDto
	{
		public required string SessionToken { get; set; }
		public required UserDto User { get; set; }
    }
}
