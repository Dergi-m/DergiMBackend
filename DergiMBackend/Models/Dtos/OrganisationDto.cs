namespace DergiMBackend.Models.Dtos
{
    public class OrganisationDto
    {
        public required string UniqueName { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        public List<ProjectDto> Projects { get; set; } = new();
    }
}