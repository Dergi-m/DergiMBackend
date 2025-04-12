using System.ComponentModel.DataAnnotations.Schema;

namespace DergiMBackend.Models.Dtos
{
	public class ProjectDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		[ForeignKey(nameof(Organisation))]
		public int OrganisationId { get; set; }
		public Organisation Organisation { get; set; }
	}
}
