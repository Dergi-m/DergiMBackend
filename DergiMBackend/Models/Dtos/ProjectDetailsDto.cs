namespace DergiMBackend.Models.Dtos
{
    public class ProjectDetailsDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
        public Guid OrganisationId { get; set; }
        public string CreatorId { get; set; }
        public List<UserDto> Members { get; set; } = new();
        public List<ProjectInvitationDto> Invitations { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
