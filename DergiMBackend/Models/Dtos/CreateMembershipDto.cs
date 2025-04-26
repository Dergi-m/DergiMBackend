using System.ComponentModel.DataAnnotations;

public class CreateMembershipDto
{
    [Required]
    public string UserId { get; set; } = default!;

    [Required]
    public Guid OrganisationId { get; set; }

    [Required]
    public Guid RoleId { get; set; }
}
