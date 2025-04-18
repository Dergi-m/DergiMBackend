using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DergiMBackend.Models
{
	public class ApplicationUser : IdentityUser
	{
        public required string Name { get; set; }
        [ForeignKey(nameof(Organisation))]
		public string? OrganisationUniqueName { get; set; }
		public required UserRole Role { get; set; }
    }
}