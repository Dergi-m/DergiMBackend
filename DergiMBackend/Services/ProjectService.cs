using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
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
                                            .Include(p => p.Invitations)
                                            .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<IEnumerable<Project>> GetProjectsForOrganisationAsync(Guid organisationId)
        {
            return await _dbContext.Projects.Where(p => p.OrganisationId == organisationId)
                                            .Include(p => p.Members)
                                            .ToListAsync();
        }

        public async Task<Project> CreateProjectAsync(CreateProjectDto createDto, string creatorId)
        {
            var organisation = await _dbContext.Organisations
                .FirstOrDefaultAsync(o => o.Id == createDto.OrganisationId)
                ?? throw new InvalidOperationException("Organisation not found.");

            var existingProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.OrganisationId == createDto.OrganisationId && p.Name == createDto.Name);

            if (existingProject != null)
                throw new InvalidOperationException("A project with the same name already exists in this organisation.");

            var creator = await _dbContext.Users
                .Include(u => u.Projects)
                .FirstOrDefaultAsync(u => u.Id == creatorId);

            if (creator == null)
                throw new InvalidOperationException("User not found.");

            var project = new Project
            {
                Id = Guid.NewGuid(),
                OrganisationId = createDto.OrganisationId,
                CreatorId = creatorId,
                Name = createDto.Name,
                Description = createDto.Description,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();

            project.Members.Add(creator);
            creator.Projects.Add(project);

            _dbContext.Projects.Update(project);
            _dbContext.Users.Update(creator);
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
            var project = await _dbContext.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return false;

            var userIds = project.Members.Select(u => u.Id).ToList();

            foreach (var userId in userIds)
            {
                var user = _dbContext.Users.Include(u => u.Projects).FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    user.Projects.Remove(project);
                    _dbContext.Users.Update(user);
                }
            }

            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task AddUserToProjectWithInvitationAsync(string projectInvitationId)
        {
            var invitation = await _dbContext.ProjectInvitations
                .Include(i => i.Project)
                .Include(i => i.TargetUser)
                .FirstOrDefaultAsync(i => i.Id == Guid.Parse(projectInvitationId));

            if (invitation == null) throw new KeyNotFoundException("No invitations with this id");

            var project = invitation.Project;
            var targetUser = invitation.TargetUser;

            if (!project.Members.Contains(targetUser))
                project.Members.Add(targetUser);
            if (!targetUser.Projects.Contains(project))
                targetUser.Projects.Add(project);

            project.Invitations.Remove(invitation);

            _dbContext.Users.Update(targetUser);
            _dbContext.Projects.Update(project);

            await _dbContext.SaveChangesAsync();
        }

        public async Task RejcetProjectInvitation(string projectInvitationId)
        {
            var invitation = await _dbContext.ProjectInvitations
                .Include(i => i.Project)
                .Include(i => i.TargetUser)
                .FirstOrDefaultAsync(i => i.Id == Guid.Parse(projectInvitationId));

            if (invitation == null) throw new KeyNotFoundException("No invitations with this id");

            var project = invitation.Project;
            var targetUser = invitation.TargetUser;

            project.Invitations.Remove(invitation);
            targetUser.ProjectInvitations.Remove(invitation);

            _dbContext.Users.Update(targetUser);
            _dbContext.Projects.Update(project);

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveUserFromProjectAsync(Guid projectId, string userId)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new KeyNotFoundException("Project not found.");

            var user = _dbContext.Users.Include(u => u.Projects).FirstOrDefault(u => u.Id == userId);

            if (user == null)
                throw new ArgumentException("This user does not exist in this project.");

            user.Projects.Remove(project);
            _dbContext.Users.Update(user);

            project.Members.Remove(user);
            _dbContext.Projects.Update(project);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> InviteUserToProjectAsync(ProjectInvitationDto dto)
        {
            var project = await _dbContext.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null)
            {
                throw new InvalidOperationException("Project not found.");
            }

            var targetUser = await _dbContext.Users
                .Include(u => u.Projects)
                .FirstOrDefaultAsync(u => u.Id == dto.TargetUserId);

            if (targetUser == null)
            {
                throw new KeyNotFoundException("Target user not found.");
            }

            var alreadyMember = targetUser.Projects.Contains(project);

            if (alreadyMember) throw new Exception("Already member of the project.");

            var invitation = new ProjectInvitation
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                Message = dto.Message,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                TargetUserId = dto.TargetUserId,
                SenderUserId = dto.SenderUserId,
            };

            _dbContext.ProjectInvitations.Add(invitation);
            targetUser.ProjectInvitations.Add(invitation);
            _dbContext.Users.Update(targetUser);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
