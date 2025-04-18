namespace DergiMBackend.Models
{
	public class Organisation
	{
		public required string UniqueName { get; set; }
		public required string Name { get; set; }
		public string? Description { get; set; }
        public ApplicationUser? Owner { get; set; }
		public required IEnumerable<ApplicationUser> Users { get; set; }
        public List<UserRole> Roles { get; set; } = new();
    }
}