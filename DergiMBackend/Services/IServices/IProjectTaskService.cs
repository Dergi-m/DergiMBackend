using DergiMBackend.Models.Dtos;
using DergiMBackend.Models;

namespace DergiMBackend.Services.IServices
{
    public interface IProjectTaskService
    {
        Task<ProjectTask> CreateProjectTaskAsync(CreateProjectTaskDto dto);
        Task<ProjectTask?> UpdateProjectTaskAsync(UpdateProjectTaskDto dto);
        Task<bool> DeleteProjectTaskAsync(Guid taskId);
        Task<ProjectTask?> GetProjectTaskByIdAsync(Guid taskId);
        Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(Guid projectId);
        Task<IEnumerable<ProjectTask>> GetTasksForUserAsync(string userId);
    }
}
