using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DergiMBackend.Models;
public enum ProjectStatus
{
    Active,
    Paused,
    Completed,
    Cancelled
}

public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public Guid OrganisationId { get; set; }

    [ForeignKey(nameof(OrganisationId))]
    public Organisation Organisation { get; set; } = default!;

    [Required]
    [MaxLength(50)]
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    public List<ApplicationUser> Members { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}