using System.ComponentModel.DataAnnotations.Schema;

namespace DergiMBackend.Models.Dtos
{
	public class LoginRequestDto
	{
		public required string UserName { get; set; }
		public required string Password { get; set; }
		[ForeignKey(nameof(Organisation))]
        public string OrganisationUniqueName { get; set; } = default!;
    }
}
