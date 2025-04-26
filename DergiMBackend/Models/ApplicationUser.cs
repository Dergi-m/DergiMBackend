using Microsoft.AspNetCore.Identity;

namespace DergiMBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = default!;
        public List<OrganisationMembership> OrganisationMemberships { get; set; } = new();
    }
}