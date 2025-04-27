using Microsoft.AspNetCore.Identity;

namespace DergiMBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string Name { get; set; } // Full name
        public int? Age { get; set; } // New field
        public string? Gender { get; set; } // New field

        // Navigation Properties if you want
        public ICollection<OrganisationMembership> OrganisationMemberships { get; set; } = new List<OrganisationMembership>();
    }
}
