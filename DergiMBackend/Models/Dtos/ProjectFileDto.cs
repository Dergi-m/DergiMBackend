using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DergiMBackend.Models.Dtos
{
	public class ProjectFileDto
	{
		public int Id { get; set; }
		public string FileUrl { get; set; }
		public int ProjectId { get; set; }
		public IFormFile File { get; set; }
	}
}
