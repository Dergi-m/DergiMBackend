using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrganisationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Organisation>> GetAllAsync()
        {
            return await _dbContext.Organisations
                .Include(o => o.OrganisationRoles)
                .Include(o => o.OrganisationMemberships)
                .ToListAsync();
        }

        public async Task<Organisation?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Organisations
                .Include(o => o.OrganisationRoles)
                .Include(o => o.OrganisationMemberships)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Organisation?> GetByUniqueNameAsync(string uniqueName)
        {
            return await _dbContext.Organisations
                .Include(o => o.OrganisationRoles)
                .Include(o => o.OrganisationMemberships)
                .FirstOrDefaultAsync(o => o.UniqueName == uniqueName);
        }

        public async Task<Organisation> CreateAsync(Organisation organisation)
        {
            _dbContext.Organisations.Add(organisation);
            await _dbContext.SaveChangesAsync();
            return organisation;
        }

        public async Task<Organisation> UpdateAsync(Organisation organisation)
        {
            _dbContext.Organisations.Update(organisation);
            await _dbContext.SaveChangesAsync();
            return organisation;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var organisation = await _dbContext.Organisations.FindAsync(id);
            if (organisation == null) return false;

            _dbContext.Organisations.Remove(organisation);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
