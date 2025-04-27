namespace DergiMBackend.Models.Dtos
{
    public class JoinOrganisationRequestDto
    {
        public string OrganisationUniqueName { get; set; } = default!;
        public Guid RoleId { get; set; }
    }

    public class LeaveOrganisationRequestDto
    {
        public string OrganisationUniqueName { get; set; } = default!;
    }
}
