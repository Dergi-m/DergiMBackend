using DergiMBackend.Models;

public class ProjectInvitationDto
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;

    public string SenderUserId { get; set; } = string.Empty;
    public string SenderUserName { get; set; } = string.Empty;

    public string TargetUserId { get; set; } = string.Empty;
    public string TargetUserName { get; set; } = string.Empty;

    public string? Message { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
