namespace DergiMBackend.Models
{
    public class OrganisationMembership
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public Guid OrganisationId { get; set; }
        public Organisation Organisation { get; set; } = default!;

        public Guid RoleId { get; set; }
        public OrganisationRole Role { get; set; } = default!;
    }
}
