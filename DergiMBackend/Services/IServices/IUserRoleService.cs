using DergiMBackend.Models;

namespace DergiMBackend.Services.IServices
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRole>> GetAllRolesAsync(string organisationUniqueName);
        Task<UserRole?> GetRoleByIdAsync(Guid id);
        Task<UserRole> CreateRoleAsync(UserRole role);
        Task<UserRole> UpdateRoleAsync(UserRole updatedRole);
        Task<bool> DeleteRoleAsync(Guid id);
    }
}