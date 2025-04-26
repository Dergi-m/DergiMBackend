using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;

namespace DergiMBackend.Services.IServices
{
    public interface IOrganisationMembershipService
    {
        Task<List<OrganisationMembership>> GetMembershipsForOrganisationAsync(Guid organisationId);
        Task<List<OrganisationMembership>> GetMembershipsForUserAsync(string userId);
        Task<OrganisationMembership?> GetMembershipByIdAsync(Guid membershipId);
        Task<OrganisationMembership> CreateMembershipAsync(CreateMembershipDto dto);
        Task<bool> DeleteMembershipAsync(Guid membershipId);
        Task SaveChangesAsync();
    }
}
