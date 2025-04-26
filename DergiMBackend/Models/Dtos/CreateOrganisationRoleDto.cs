using System.ComponentModel.DataAnnotations;

public class CreateOrganisationRoleDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }
    public bool CanAssignTasks { get; set; }
    public bool CanCreateTasks { get; set; }
    public List<string> VisibleTags { get; set; } = new();
}
