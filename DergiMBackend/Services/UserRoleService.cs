using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly ApplicationDbContext _context;

        public UserRoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRole>> GetAllRolesAsync(string organisationUniqueName)
        {
            var roles = await _context.OrganisationRoles
                .Where(r => r.OrganisationUniqueName == organisationUniqueName)
                .ToListAsync();

            return roles;
        }

        public async Task<UserRole?> GetRoleByIdAsync(Guid id)
        {
            return await _context.OrganisationRoles.FindAsync(id);
        }

        public async Task<UserRole> CreateRoleAsync(UserRole role)
        {
            // Validate organisation exists
            var organisation = await _context.Organisations.FirstOrDefaultAsync(o => o.UniqueName == role.OrganisationUniqueName);
            if (organisation == null)
                throw new InvalidOperationException("Organisation not found.");

            _context.OrganisationRoles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteRoleAsync(Guid id)
        {
            var role = await _context.OrganisationRoles.FindAsync(id);
            if (role == null) return false;

            _context.OrganisationRoles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserRole> UpdateRoleAsync(UserRole updatedRole)
        {
            var existing = await _context.OrganisationRoles.FindAsync(updatedRole.Id);
            if (existing == null)
                throw new KeyNotFoundException("Role not found.");

            // Update properties
            existing.Name = updatedRole.Name;
            existing.VisibleTags = updatedRole.VisibleTags;
            existing.CanAssignTasks = updatedRole.CanAssignTasks;
            existing.CanCreateTasks = updatedRole.CanCreateTasks;
            existing.Description = updatedRole.Description;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}