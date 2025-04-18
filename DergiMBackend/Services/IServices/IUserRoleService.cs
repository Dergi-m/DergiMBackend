using DergiMBackend.Models;

namespace DergiMBackend.Services.IServices;
public interface IUserRoleService
{
    Task<IEnumerable<UserRole>> GetAllRolesAsync();
    Task<UserRole?> GetRoleByIdAsync(Guid id);
    Task<UserRole> CreateRoleAsync(UserRole role);
    Task<bool> DeleteRoleAsync(Guid id);
    Task<UserRole> UpdateRoleAsync(UserRole updatedRole);
}