namespace DergiMBackend.Models.Dtos
{
    public class UpdateOrganisationRoleDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool CanAssignTasks { get; set; }
        public bool CanCreateTasks { get; set; }
        public List<string> VisibleTags { get; set; } = new();
    }
}
