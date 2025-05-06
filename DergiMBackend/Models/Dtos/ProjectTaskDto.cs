namespace DergiMBackend.Models.Dtos
{
    public class ProjectTaskDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public string? AssignedToUserId { get; set; }
        public UserDto? AssignedToUser { get; set; }
        public List<Guid> AttachedFileIds { get; set; } = new();
        public List<ProjectFileDto> AttachedFiles { get; set; } = new();
        public Guid ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
