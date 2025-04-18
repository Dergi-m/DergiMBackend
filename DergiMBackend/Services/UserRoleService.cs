using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.EntityFrameworkCore;

public class UserRoleService : IUserRoleService
{
    private readonly ApplicationDbContext _context;

    public UserRoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRole>> GetAllRolesAsync() => await _context.UserRoles.ToListAsync();

    public async Task<UserRole?> GetRoleByIdAsync(Guid id) => await _context.UserRoles.FindAsync(id);

    public async Task<UserRole> CreateRoleAsync(UserRole role)
    {
        _context.UserRoles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _context.UserRoles.FindAsync(id);
        if (role == null) return false;
        _context.UserRoles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserRole> UpdateRoleAsync(UserRole updatedRole)
    {
        var existing = await _context.UserRoles.FindAsync(updatedRole.Id);
        if (existing == null) throw new KeyNotFoundException();

        existing.Name = updatedRole.Name;
        existing.VisibleTags = updatedRole.VisibleTags;
        existing.CanAssignTasks = updatedRole.CanAssignTasks;
        existing.CanCreateTasks = updatedRole.CanCreateTasks;
        existing.Description = updatedRole.Description;

        await _context.SaveChangesAsync();
        return existing;
    }
}