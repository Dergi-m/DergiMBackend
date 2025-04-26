namespace DergiMBackend.Models
{
    public class Organisation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UniqueName { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public string OwnerId { get; set; } = default!;
        public ApplicationUser Owner { get; set; } = default!;

        public ICollection<OrganisationMembership> Members { get; set; } = new List<OrganisationMembership>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<OrganisationRole> Roles { get; set; } = new List<OrganisationRole>();
    }
}