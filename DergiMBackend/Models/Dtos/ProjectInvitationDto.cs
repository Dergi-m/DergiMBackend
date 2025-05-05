using DergiMBackend.Models;

public class ProjectInvitationDto
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string SenderUserId { get; set; } = string.Empty;
    public string TargetUserId { get; set; } = string.Empty;

    public string? Message { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
