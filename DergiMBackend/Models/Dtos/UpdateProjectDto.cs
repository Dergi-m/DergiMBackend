namespace DergiMBackend.Models.Dtos
{
    public class UpdateProjectDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus? Status { get; set; }
    }
}
