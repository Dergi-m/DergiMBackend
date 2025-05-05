using System.ComponentModel.DataAnnotations;

public class CreateOrganisationDto
{
    [Required]
    [MinLength(3)]
    public string UniqueName { get; set; } = default!;

    [Required]
    [MinLength(3)]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }
    public Guid OwnerId { get; set; } = default!;
}
