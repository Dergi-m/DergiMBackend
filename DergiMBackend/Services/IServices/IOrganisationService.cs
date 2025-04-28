using DergiMBackend.Models;

namespace DergiMBackend.Services.IServices
{
    public interface IOrganisationService
    {
        Task<IEnumerable<Organisation>> GetAllOrganisationsAsync();
        Task<Organisation?> GetOrganisationByIdAsync(Guid id);
        Task<Organisation?> GetOrganisationByUniqueNameAsync(string uniqueName);
        Task<Organisation> CreateOrganisationAsync(string uniqueName, string name, string? description, ApplicationUser owner);
        Task<Organisation?> UpdateOrganisationAsync(Guid organisationId, string? name, string? description);
        Task<bool> DeleteOrganisationAsync(Guid id);
    }
}
