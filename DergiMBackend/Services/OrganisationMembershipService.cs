using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
    public class OrganisationMembershipService : IOrganisationMembershipService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrganisationMembershipService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OrganisationMembership>> GetMembershipsForOrganisationAsync(Guid organisationId)
        {
            return await _dbContext.OrganisationMemberships
                .Include(m => m.User)
                .Include(m => m.Role)
                .Where(m => m.OrganisationId == organisationId)
                .ToListAsync();
        }

        public async Task<OrganisationMembership?> GetMembershipByIdAsync(Guid membershipId)
        {
            return await _dbContext.OrganisationMemberships
                .Include(m => m.User)
                .Include(m => m.Role)
                .FirstOrDefaultAsync(m => m.Id == membershipId);
        }

        public async Task<OrganisationMembership> CreateMembershipAsync(CreateMembershipDto dto)
        {
            var membership = new OrganisationMembership
            {
                UserId = dto.UserId,
                OrganisationId = dto.OrganisationId,
                RoleId = dto.RoleId
            };

            _dbContext.OrganisationMemberships.Add(membership);
            await _dbContext.SaveChangesAsync();
            return membership;
        }

        public async Task<bool> DeleteMembershipAsync(Guid membershipId)
        {
            var membership = await _dbContext.OrganisationMemberships.FindAsync(membershipId);
            if (membership == null) return false;

            _dbContext.OrganisationMemberships.Remove(membership);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<OrganisationMembership>> GetMembershipsForUserAsync(string userId)
        {
            return await _dbContext.OrganisationMemberships
                .Include(m => m.Organisation)
                .Include(m => m.Role)
                .Where(m => m.UserId == userId)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
