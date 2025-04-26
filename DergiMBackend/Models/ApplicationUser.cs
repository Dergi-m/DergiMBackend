using Microsoft.AspNetCore.Identity;

namespace DergiMBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = default!;
        public ICollection<OrganisationMembership> Memberships { get; set; } = new List<OrganisationMembership>();
    }
}