namespace DergiMBackend.Models
{
    public class Organisation
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Primary key

        public required string UniqueName { get; set; } // example: "aybuproject"

        public required string Name { get; set; }

        public string? Description { get; set; }

        public List<OrganisationMembership> OrganisationMemberships { get; set; } = new();

        public List<OrganisationRole> OrganisationRoles { get; set; } = new();
    }
}
