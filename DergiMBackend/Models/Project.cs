using System.ComponentModel.DataAnnotations.Schema;

namespace DergiMBackend.Models
{
	public class Project
	{
		public int Id { get; set; }
		public required string Name { get; set; }
		public required string Description { get; set; }
		[ForeignKey(nameof(Organisation))]
		public int OrganisationId { get; set; }
		public required IEnumerable<ApplicationUser> Users { get; set; }
		public required IEnumerable<ProjectFile> ProjectFiles { get; set; }
	}
}
