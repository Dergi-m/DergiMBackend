namespace DergiMBackend.Models.Dtos
{
    public class UpdateProjectTaskDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public TaskStatus? Status { get; set; }
        public string? AssignedToUserId { get; set; }
        public List<Guid>? AttachedFileIds { get; set; }
    }
}
