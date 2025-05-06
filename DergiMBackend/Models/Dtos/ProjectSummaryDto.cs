namespace DergiMBackend.Models.Dtos
{
    public class ProjectSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }

}
