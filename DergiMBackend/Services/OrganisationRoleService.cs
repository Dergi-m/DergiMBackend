using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
    public class OrganisationRoleService : IOrganisationRoleService
    {
        private readonly ApplicationDbContext _context;

        public OrganisationRoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrganisationRole>> GetRolesByOrganisationAsync(Guid organisationId)
        {
            return await _context.OrganisationRoles
                .Where(r => r.OrganisationId == organisationId)
                .ToListAsync();
        }

        public async Task<OrganisationRole> CreateRoleAsync(Guid organisationId, OrganisationRole role)
        {
            role.OrganisationId = organisationId;
            _context.OrganisationRoles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<OrganisationRole> UpdateRoleAsync(Guid roleId, OrganisationRole updatedRole)
        {
            var existingRole = await _context.OrganisationRoles.FindAsync(roleId);

            if (existingRole == null)
                throw new KeyNotFoundException("Role not found.");

            existingRole.Name = updatedRole.Name;
            existingRole.Description = updatedRole.Description;
            existingRole.CanAssignTasks = updatedRole.CanAssignTasks;
            existingRole.CanCreateTasks = updatedRole.CanCreateTasks;
            existingRole.VisibleTags = updatedRole.VisibleTags;

            await _context.SaveChangesAsync();
            return existingRole;
        }

        public async Task<bool> DeleteRoleAsync(Guid roleId)
        {
            var role = await _context.OrganisationRoles.FindAsync(roleId);
            if (role == null)
                return false;

            _context.OrganisationRoles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
