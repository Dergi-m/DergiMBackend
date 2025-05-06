namespace DergiMBackend.Models.Dtos
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public List<OrganisationMembershipDto> OrganisationMemberships { get; set; } = new();
        public List<ProjectInvitationDto> ProjectInvitations { get; set; } = new();
        public List<ProjectSummaryDto> Projects { get; set; } = new();
    }
}
