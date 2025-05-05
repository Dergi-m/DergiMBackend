namespace DergiMBackend.Models.Dtos
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public List<ProjectInvitationDto> ProjectInvitations { get; set; } = new();
        public List<ProjectDto> Projects { get; set; } = new();
        public int? Age { get; set; }
        public string? Gender { get; set; }
    }
}
