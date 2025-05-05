// Models/ProjectInvitation.cs
using System.ComponentModel.DataAnnotations;

namespace DergiMBackend.Models
{
    public class ProjectInvitation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public string SenderUserId { get; set; } = default!;
        public ApplicationUser SenderUser { get; set; } = default!;

        public string TargetUserId { get; set; } = default!;
        public ApplicationUser TargetUser { get; set; } = default!;

        public string? Message { get; set; } = "You have been invited to join a project!";
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public DateTime CreatedAt { get; set; }
    }

    public enum InvitationStatus
    {
        Pending,
        Accepted,
        Declined
    }
}
