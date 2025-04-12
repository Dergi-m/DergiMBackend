using System.Text.Json.Serialization;

namespace DergiMBackend.Models
{
	public class Organisation
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public IEnumerable<ApplicationUser> Users { get; set; }
	}
}
