using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DergiMBackend.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string Name { get; set; }
		[ForeignKey(nameof(Organisation))]
		public int? OrganisationId { get; set; }
		public Organisation? Organisation { get; set; }
		public IEnumerable<Project> Projects { get; set; }
	}
}
