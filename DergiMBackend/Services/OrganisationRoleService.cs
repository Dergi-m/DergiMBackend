using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
    public class OrganisationRoleService : IOrganisationRoleService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrganisationRoleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OrganisationRole>> GetRolesForOrganisationAsync(Guid organisationId)
        {
            return await _dbContext.OrganisationRoles
                .Where(r => r.OrganisationId == organisationId)
                .ToListAsync();
        }

        public async Task<OrganisationRole?> GetRoleByIdAsync(Guid roleId)
        {
            return await _dbContext.OrganisationRoles
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task<OrganisationRole> CreateRoleAsync(Guid organisationId, OrganisationRole role)
        {
            role.OrganisationId = organisationId;
            _dbContext.OrganisationRoles.Add(role);
            await _dbContext.SaveChangesAsync();
            return role;
        }

        public async Task<OrganisationRole> UpdateRoleAsync(OrganisationRole role)
        {
            var existing = await _dbContext.OrganisationRoles.FindAsync(role.Id);
            if (existing == null)
                throw new KeyNotFoundException("Role not found");

            existing.Name = role.Name;
            existing.Description = role.Description;
            existing.VisibleTags = role.VisibleTags;
            existing.CanAssignTasks = role.CanAssignTasks;
            existing.CanCreateTasks = role.CanCreateTasks;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteRoleAsync(Guid roleId)
        {
            var role = await _dbContext.OrganisationRoles.FindAsync(roleId);
            if (role == null)
                return false;

            _dbContext.OrganisationRoles.Remove(role);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
