using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DergiMBackend.Models
{
    public class ProjectTask
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Created;

        public string? AssignedToUserId { get; set; }

        [ForeignKey(nameof(AssignedToUserId))]
        public ApplicationUser? AssignedToUser { get; set; }

        public ICollection<ProjectFile> AttachedFiles { get; set; } = new List<ProjectFile>();

        [ForeignKey(nameof(Project))]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum TaskStatus
    {
        Created = 0,
        Ongoing = 1,
        Reviewing = 2,
        Finished = 3
    }

}
