namespace DergiMBackend.Models
{
    public class Organisation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string UniqueName { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }

        public List<OrganisationMembership> OrganisationMemberships { get; set; } = new();
        public List<OrganisationRole> OrganisationRoles { get; set; } = new();
    }
}
