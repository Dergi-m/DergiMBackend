namespace DergiMBackend.Models.Dtos
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;
        public Guid OrganisationId { get; set; }
        public List<string> MemberIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
