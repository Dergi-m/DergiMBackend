using DergiMBackend.Models;

namespace DergiMBackend.Services.IServices
{
    public interface IOrganisationService
    {
        Task<List<Organisation>> GetAllAsync();
        Task<Organisation?> GetByIdAsync(Guid id);
        Task<Organisation?> GetByUniqueNameAsync(string uniqueName);
        Task<Organisation> CreateAsync(Organisation organisation);
        Task<Organisation> UpdateAsync(Organisation organisation);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
