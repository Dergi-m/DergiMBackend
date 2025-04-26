namespace DergiMBackend.Models
{
    public class OrganisationRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        
        public bool CanAssignTasks { get; set; }
        public bool CanCreateTasks { get; set; }
        public List<string> VisibleTags { get; set; } = new();
    }
}