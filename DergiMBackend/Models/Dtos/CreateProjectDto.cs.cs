namespace DergiMBackend.Models.Dtos
{
    public class CreateProjectDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Guid OrganisationId { get; set; }
    }
}
