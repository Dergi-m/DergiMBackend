using Microsoft.AspNetCore.Identity;

namespace DergiMBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string Name { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public ICollection<OrganisationMembership> OrganisationMemberships { get; set; } = new List<OrganisationMembership>();
        public ICollection<ProjectInvitation> ProjectInvitations { get; set; } = new List<ProjectInvitation>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
