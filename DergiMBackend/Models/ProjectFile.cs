using System.ComponentModel.DataAnnotations.Schema;

namespace DergiMBackend.Models
{
	public class ProjectFile
	{
		public int Id { get; set; }
		public string FileUrl { get; set; }
		public string LocalFileUrl { get; set; }
		[ForeignKey(nameof(Project))]
		public int ProjectId { get; set; }
		public Project Project { get; set; }
		public IEnumerable<ProjectFile> ProjectFiles { get; set; }
	}
}
