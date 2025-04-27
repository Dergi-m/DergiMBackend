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

        public async Task<IEnumerable<Organisation>> GetAllOrganisationsAsync()
        {
            return await _dbContext.Organisations
                .Include(o => o.OrganisationRoles)
                .Include(o => o.OrganisationMemberships)
                .ToListAsync();
        }

        public async Task<Organisation?> GetOrganisationByIdAsync(Guid id)
        {
            return await _dbContext.Organisations
                .Include(o => o.OrganisationRoles)
                .Include(o => o.OrganisationMemberships)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Organisation?> GetOrganisationByUniqueNameAsync(string uniqueName)
        {
            return await _dbContext.Organisations
                .Include(o => o.OrganisationRoles)
                .Include(o => o.OrganisationMemberships)
                .FirstOrDefaultAsync(o => o.UniqueName == uniqueName);
        }

        public async Task<Organisation> CreateOrganisationAsync(string uniqueName, string name, string? description, ApplicationUser owner)
        {
            var organisation = new Organisation
            {
                UniqueName = uniqueName,
                Name = name,
                Description = description,
                Owner = owner,
                OrganisationRoles = new List<OrganisationRole>(),
                OrganisationMemberships = new List<OrganisationMembership>()
            };

            _dbContext.Organisations.Add(organisation);
            await _dbContext.SaveChangesAsync();
            return organisation;
        }

        public async Task<Organisation?> UpdateOrganisationAsync(Guid organisationId, string? name, string? description)
        {
            var organisation = await _dbContext.Organisations.FindAsync(organisationId);

            if (organisation == null)
                return null;

            if (!string.IsNullOrWhiteSpace(name))
                organisation.Name = name;

            if (!string.IsNullOrWhiteSpace(description))
                organisation.Description = description;

            _dbContext.Organisations.Update(organisation);
            await _dbContext.SaveChangesAsync();
            return organisation;
        }

        public async Task<bool> DeleteOrganisationAsync(Guid id)
        {
            var organisation = await _dbContext.Organisations.FindAsync(id);
            if (organisation == null)
                return false;

            _dbContext.Organisations.Remove(organisation);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
