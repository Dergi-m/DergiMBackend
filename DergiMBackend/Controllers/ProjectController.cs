using AutoMapper;
using DergiMBackend.Authorization;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DergiMBackend.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ISessionService _sessionService;
        private readonly IMapper _mapper;

        public ProjectController(IProjectService projectService, ISessionService sessionService, IMapper mapper)
        {
            _projectService = projectService;
            _sessionService = sessionService;
            _mapper = mapper;
        }

        [HttpGet("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound();
            
            var dto = _mapper.Map<ProjectDetailsDto>(project);

            return Ok(dto);
        }

        [HttpGet("organisation/{organisationId:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetProjectsForOrganisation(Guid organisationId)
        {
            var projects = await _projectService.GetProjectsForOrganisationAsync(organisationId);
            return Ok(projects);
        }

        [HttpPost]
        [SessionAuthorize]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createDto)
        {
            var token = HttpContext.Request.Headers["SessionToken"].FirstOrDefault();


            if (token == null) throw new UnauthorizedAccessException("Session token is missing");

            var sessionUser = await _sessionService.ValidateSessionTokenAsync(token);
            var currentUserId = sessionUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == null) throw new UnauthorizedAccessException("User not found.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _projectService.CreateProjectAsync(createDto, currentUserId);
            var created = await _projectService.GetProjectByIdAsync(project.Id) ?? throw new Exception("Something went wrong please try again later");
            return Ok(created);
        }

        [HttpPut]
        [SessionAuthorize]
        public async Task<IActionResult> UpdateProject([FromBody] UpdateProjectDto updateDto)
        {
            var token = HttpContext.Request.Headers["SessionToken"].FirstOrDefault();
            
            if(token == null) throw new UnauthorizedAccessException("Session token is missing");
            

            var sessionUser = await _sessionService.ValidateSessionTokenAsync(token);
            var currentUserId = sessionUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _projectService.GetProjectByIdAsync(updateDto.Id);

            if(project == null) return NotFound(new { message = "Project not found" });
            
            if (project.CreatorId != currentUserId) return Unauthorized();

            var updatedProject = await _projectService.UpdateProjectAsync(updateDto);

            return Ok(updatedProject);
        }


        [HttpDelete("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var deleted = await _projectService.DeleteProjectAsync(id);
            if (!deleted)
                return NotFound();

            return Ok(deleted);
        }

        [HttpPut("{projectId:guid}/accept-invitation")]
        [SessionAuthorize]
        public async Task<IActionResult> AcceptInvitation(Guid projectId, string invitationId)
        {
            if (invitationId == null || invitationId == "")
            {
                return BadRequest("Invitation ID must be provided.");
            }

            var token = HttpContext.Request.Headers["SessionToken"].FirstOrDefault();

            if (token == null) throw new UnauthorizedAccessException("Session token is missing");


            var sessionUser = await _sessionService.ValidateSessionTokenAsync(token);
            var currentUserId = sessionUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            var invitation = project.Invitations.FirstOrDefault(i => i.Id == Guid.Parse(invitationId));

            if (invitation == null)
                return NotFound(new { message = "Invitation not found" });

            if (currentUserId != invitation.TargetUserId)
                return Unauthorized(new { message = "You are not authorized to accept this invitation." });

            await _projectService.AddUserToProjectWithInvitationAsync(invitationId);

            var updatedMembers = project.Members.Select(m => new UserDto
            {
                Id = m.Id,
                Name = m.Name,
                UserName = m.UserName ?? "",
                Email = m.Email ?? "",
            }).ToList();

            return Ok(new { Success = true, Message = "Users added to added project successfully.", UpdatedMembers = updatedMembers });
        }

        [HttpPut("{projectId:guid}/reject-invitation")]
        [SessionAuthorize]
        public async Task<IActionResult> RejectInvitation(Guid projectId, string invitationId)
        {
            if (invitationId == null || invitationId == "")
            {
                return BadRequest("Invitation ID must be provided.");
            }

            var token = HttpContext.Request.Headers["SessionToken"].FirstOrDefault();

            if (token == null) throw new UnauthorizedAccessException("Session token is missing");

            var sessionUser = await _sessionService.ValidateSessionTokenAsync(token);
            var currentUserId = sessionUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            var invitation = project.Invitations.FirstOrDefault(i => i.Id == Guid.Parse(invitationId));

            if (invitation == null)
                return NotFound(new { message = "Invitation not found" });

            if (currentUserId != invitation.TargetUserId)
                return Unauthorized(new { message = "You are not authorized to accept this invitation." });

            await _projectService.RejcetProjectInvitation(invitationId);

            var updatedMembers = project.Members.Select(m => new UserDto
            {
                Id = m.Id,
                Name = m.Name,
                UserName = m.UserName ?? "",
                Email = m.Email ?? "",
            }).ToList();

            return Ok(new { Success = true, Message = "Invitation rejeceted successfully.", UpdatedMembers = updatedMembers });
        }

        [HttpPut("{projectId:guid}/remove-member")]
        [SessionAuthorize]
        public async Task<IActionResult> RemoveUserFromProject(Guid projectId, string userId)
        {
         
            if (userId == null || userId == "")
            {
                return BadRequest("User ID must be provided.");
            }

            var token = HttpContext.Request.Headers["SessionToken"].FirstOrDefault();

            if (token == null) throw new UnauthorizedAccessException("Session token is missing");


            var sessionUser = await _sessionService.ValidateSessionTokenAsync(token);
            var currentUserId = sessionUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            if (project.CreatorId != currentUserId) return Unauthorized();


            await _projectService.RemoveUserFromProjectAsync(projectId, userId);


            var updatedMembers = project.Members.Select(m => new UserDto
            {
                Id = m.Id,
                Name = m.Name,
                UserName = m.UserName ?? "",
                Email = m.Email ?? "",
            }).ToList();


            return Ok(new { Success = true, Message = "Users removed from the project successfully.", UpdatedMembers = updatedMembers });
        }

        [HttpPost("invite")]
        [SessionAuthorize]
        public async Task<IActionResult> InviteUserToProject([FromBody] CreateProjectInvitationDto createDto)
        {
            try
            {
                var token = HttpContext.Request.Headers["SessionToken"].FirstOrDefault();

                if (token == null) throw new UnauthorizedAccessException("Session token is missing");

                var sessionUser = await _sessionService.ValidateSessionTokenAsync(token);
                var currentUserId = sessionUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var project = await _projectService.GetProjectByIdAsync(createDto.ProjectId);

                if(project == null)
                    return NotFound(new { Success = false, Message = "Project not found.", StatusCode = 404 });

                if (project.CreatorId != currentUserId)
                {
                    return Unauthorized(new { Success = false, Message = "You are not authorized to invite users to this project.", StatusCode = 401 });
                }

                ProjectInvitationDto invitation = new ProjectInvitationDto
                {
                    Id = new Guid(),
                    Message= createDto.Message,
                    ProjectId = createDto.ProjectId,
                    TargetUserId = createDto.TargetUserId,
                    SenderUserId = currentUserId,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _projectService.InviteUserToProjectAsync(invitation);

                if (!result)
                    return BadRequest(new { Success = false, Message = "Failed to create invitation.", StatusCode = 400 });

                return Ok(new { Success = true, Message = "Invitation sent successfully." });

            } catch(Exception ex)
            {
                if(ex.GetType() == typeof(KeyNotFoundException))
                {
                    return NotFound(new { Success = false, Message = ex.Message, StatusCode = 404 });
                }

                return BadRequest(new { Success = false, Message = ex.Message, StatusCode = 400 });
            }
            
        }

    }
}
