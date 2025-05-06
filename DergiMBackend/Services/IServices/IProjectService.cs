using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;

namespace DergiMBackend.Services.IServices
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(Guid projectId);
        Task<IEnumerable<Project>> GetProjectsForOrganisationAsync(Guid organisationId);
        Task<Project> CreateProjectAsync(CreateProjectDto createDto, string creatorId);
        Task<Project?> UpdateProjectAsync(UpdateProjectDto updateDto);
        Task<bool> DeleteProjectAsync(Guid projectId);
        Task AddUserToProjectWithInvitationAsync(string projectInvitationId);
        Task RejcetProjectInvitation(string projectInvitationId);
        Task RemoveUserFromProjectAsync(Guid projectId, string userIds);
        Task<bool> InviteUserToProjectAsync(ProjectInvitationDto dto);

        Task SaveChangesAsync();
    }
}
