using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DergiMBackend.Models
{
    public class ProjectFile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string FileUrl { get; set; } = default!;

        [Required]
        public string LocalFileUrl { get; set; } = default!;

        [ForeignKey(nameof(Project))]
        public Guid ProjectId { get; set; }

        public Project Project { get; set; } = default!;

        [JsonIgnore]
        public ICollection<ProjectTask>? ProjectTasks { get; set; } = new List<ProjectTask>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
