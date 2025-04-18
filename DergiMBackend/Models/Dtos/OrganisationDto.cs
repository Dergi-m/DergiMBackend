namespace DergiMBackend.Models.Dtos
{
	public class OrganisationDto
	{
		public required string UniqueName { get; set; }
		public string? Description { get; set; }
		public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public List<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
    }
}
