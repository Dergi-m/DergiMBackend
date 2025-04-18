using System.ComponentModel.DataAnnotations.Schema;

namespace DergiMBackend.Models
{
	public class ProjectFile
	{
		public int Id { get; set; }
		public required string FileUrl { get; set; }
		public required string LocalFileUrl { get; set; }
		[ForeignKey(nameof(Project))]
		public int ProjectId { get; set; }
		public required Project Project { get; set; }
	}
}
