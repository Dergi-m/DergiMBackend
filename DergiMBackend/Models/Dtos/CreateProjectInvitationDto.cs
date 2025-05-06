using DergiMBackend.Models;

public class CreateProjectInvitationDto
{
    public Guid ProjectId { get; set; }
    public string TargetUserId { get; set; } = string.Empty;
    public string? Message { get; set; }
}
