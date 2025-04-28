namespace DergiMBackend.Models.Dtos
{
    public class AddProjectMembersDto
    {
        public required Guid ProjectId { get; set; }
        public List<string> MemberIds { get; set; } = new();
    }
}
