using System.ComponentModel.DataAnnotations.Schema;

namespace DergiMBackend.Models.Dtos
{
	public class ProjectDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int OrganisationId { get; set; }
		public IEnumerable<ProjectFileDto>? ProjectFiles { get; set; }
	}
}
