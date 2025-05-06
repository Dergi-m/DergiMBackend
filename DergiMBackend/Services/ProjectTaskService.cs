using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProjectTaskService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProjectTask> CreateProjectTaskAsync(CreateProjectTaskDto dto)
        {
            var task = new ProjectTask
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status,
                AssignedToUserId = dto.AssignedToUserId,
                ProjectId = dto.ProjectId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Attach existing files by ID
            if (dto.AttachedFileIds != null && dto.AttachedFileIds.Any())
            {
                var files = await _dbContext.ProjectFiles
                    .Where(f => dto.AttachedFileIds.Contains(f.Id))
                    .ToListAsync();

                task.AttachedFiles = files;
            }

            _dbContext.ProjectTasks.Add(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task<ProjectTask?> UpdateProjectTaskAsync(UpdateProjectTaskDto dto)
        {
            var task = await _dbContext.ProjectTasks
                .Include(t => t.AttachedFiles)
                .FirstOrDefaultAsync(t => t.Id == dto.Id);

            if (task == null) return null;

            task.Name = dto.Name;
            task.Description = dto.Description;
            task.Status = dto.Status ?? task.Status;
            task.AssignedToUserId = dto.AssignedToUserId;
            task.UpdatedAt = DateTime.UtcNow;

            if (dto.AttachedFileIds != null)
            {
                var files = await _dbContext.ProjectFiles
                    .Where(f => dto.AttachedFileIds.Contains(f.Id))
                    .ToListAsync();

                task.AttachedFiles = files;
            }

            _dbContext.ProjectTasks.Update(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteProjectTaskAsync(Guid taskId)
        {
            var task = await _dbContext.ProjectTasks.FindAsync(taskId);
            if (task == null) return false;

            _dbContext.ProjectTasks.Remove(task);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectTask?> GetProjectTaskByIdAsync(Guid taskId)
        {
            return await _dbContext.ProjectTasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.AttachedFiles)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(Guid projectId)
        {
            return await _dbContext.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AttachedFiles)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksForUserAsync(string userId)
        {
            return await _dbContext.ProjectTasks
                .Where(t => t.AssignedToUserId == userId)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AttachedFiles)
                .ToListAsync();
        }
    }
}
