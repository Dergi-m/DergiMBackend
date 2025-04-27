using DergiMBackend.Models;

namespace DergiMBackend.Services.IServices
{
    public interface IOrganisationRoleService
    {
        Task<List<OrganisationRole>> GetRolesForOrganisationAsync(Guid organisationId);
        Task<OrganisationRole?> GetRoleByIdAsync(Guid roleId);
        Task<OrganisationRole> CreateRoleAsync(Guid organisationId, OrganisationRole role);
        Task<OrganisationRole> UpdateRoleAsync(OrganisationRole role);
        Task<bool> DeleteRoleAsync(Guid roleId);
    }
}
