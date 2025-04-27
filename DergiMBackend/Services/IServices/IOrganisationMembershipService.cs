using DergiMBackend.Models;

namespace DergiMBackend.Services.IServices
{
    public interface IOrganisationMembershipService
    {
        Task<List<OrganisationMembership>> GetMembershipsForOrganisationAsync(Guid organisationId);
        Task<List<OrganisationMembership>> GetMembershipsForUserAsync(string userId);
        Task<OrganisationMembership?> GetMembershipByIdAsync(Guid membershipId);
        Task<OrganisationMembership> CreateMembershipAsync(OrganisationMembership membership);
        Task<bool> DeleteMembershipAsync(Guid membershipId);
    }
}
