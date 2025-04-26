namespace DergiMBackend.Models.Dtos
{
    public class OrganisationMembershipDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = default!;
        public Guid OrganisationId { get; set; }
        public Guid RoleId { get; set; }
    }
}
