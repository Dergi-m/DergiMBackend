namespace DergiMBackend.Models.Dtos
{
    public class CreateProjectTaskDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Guid ProjectId { get; set; }
        public string? AssignedToUserId { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Created;
        public List<Guid> AttachedFileIds { get; set; } = new();
    }
}
