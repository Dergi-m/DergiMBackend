using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;

namespace DergiMBackend.Services.IServices
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(Guid projectId);
        Task<IEnumerable<Project>> GetProjectsForOrganisationAsync(Guid organisationId);
        Task<Project> CreateProjectAsync(CreateProjectDto createDto);
        Task<Project?> UpdateProjectAsync(UpdateProjectDto updateDto);
        Task<bool> DeleteProjectAsync(Guid projectId);
        Task AddUsersToProjectAsync(Guid projectId, List<string> userIds);
        Task RemoveUsersFromProjectAsync(Guid projectId, List<string> userIds);
        Task SaveChangesAsync();
    }
}
