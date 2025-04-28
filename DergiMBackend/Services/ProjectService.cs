using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProjectService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _dbContext.Projects.Include(p => p.Members).ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(Guid projectId)
        {
            return await _dbContext.Projects.Include(p => p.Members)
                                            .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<IEnumerable<Project>> GetProjectsForOrganisationAsync(Guid organisationId)
        {
            return await _dbContext.Projects.Where(p => p.OrganisationId == organisationId)
                                            .Include(p => p.Members)
                                            .ToListAsync();
        }

        public async Task<Project> CreateProjectAsync(CreateProjectDto createDto)
        {
            var organisation = await _dbContext.Organisations
                .FirstOrDefaultAsync(o => o.Id == createDto.OrganisationId)
                ?? throw new InvalidOperationException("Organisation not found.");

            var existingProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.OrganisationId == createDto.OrganisationId && p.Name == createDto.Name);

            if (existingProject != null)
                throw new InvalidOperationException("A project with the same name already exists in this organisation.");

            var project = new Project
            {
                Id = Guid.NewGuid(),
                OrganisationId = createDto.OrganisationId,
                Name = createDto.Name,
                Description = createDto.Description,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();

            return project;
        }



        public async Task<Project?> UpdateProjectAsync(UpdateProjectDto dto)
        {
            var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (project == null) return null;

            project.Name = dto.Name;
            project.Description = dto.Description ?? project.Description;
            if (dto.Status.HasValue)
                project.Status = dto.Status.Value;

            project.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(Guid projectId)
        {
            var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return false;

            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task AddUsersToProjectAsync(Guid projectId, List<string> userIds)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found.");

            var users = await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            if (users.Count == 0)
                throw new ArgumentException("No valid users found to add.");

            foreach (var user in users)
            {
                if (!project.Members.Contains(user))
                {
                    project.Members.Add(user);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveUsersFromProjectAsync(Guid projectId, List<string> userIds)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found.");

            var users = await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            if (users.Count == 0)
                throw new ArgumentException("No valid users found to add.");

            project.Members.RemoveAll(u => userIds.Contains(u.Id));

            await _dbContext.SaveChangesAsync();
        }


        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
