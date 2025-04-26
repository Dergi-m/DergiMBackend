using DergiMBackend.Models;

namespace DergiMBackend.Services.IServices
{
    public interface IOrganisationRoleService
    {
        Task<IEnumerable<OrganisationRole>> GetRolesByOrganisationAsync(Guid organisationId);
        Task<OrganisationRole> CreateRoleAsync(Guid organisationId, OrganisationRole role);
        Task<OrganisationRole> UpdateRoleAsync(Guid roleId, OrganisationRole role);
        Task<bool> DeleteRoleAsync(Guid roleId);
    }
}
