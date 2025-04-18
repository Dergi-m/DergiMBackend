
namespace DergiMBackend.Models;

public class UserRole
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required string OrganisationUniqueName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<string> VisibleTags { get; set; } = new();
    public bool CanAssignTasks { get; set; }
    public bool CanCreateTasks { get; set; }
    public string? Description { get; set; }

    public static implicit operator UserRole(Task<UserRole?> v)
    {
        throw new NotImplementedException();
    }
}