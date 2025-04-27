namespace DergiMBackend.Models
{
    public class OrganisationRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }

        public string? Description { get; set; }

        public bool CanAssignTasks { get; set; }

        public bool CanCreateTasks { get; set; }

        public List<string> VisibleTags { get; set; } = new();

        public Guid OrganisationId { get; set; }
        public Organisation Organisation { get; set; } = default!;
    }
}
