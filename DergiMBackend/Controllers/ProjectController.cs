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

        public ProjectController(IProjectService projectService, ISessionService sessionService)
        {
            _projectService = projectService;
            _sessionService = sessionService;
        }

        [HttpGet("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound();

            return Ok(project);
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

        [HttpPut("{projectId:guid}/add-members")]
        [SessionAuthorize]
        public async Task<IActionResult> AddUsersToProject(Guid projectId, [FromBody] List<string> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return BadRequest("User IDs must be provided.");
            }

            var token = HttpContext.Request.Headers["SessionToken"].FirstOrDefault();

            if (token == null) throw new UnauthorizedAccessException("Session token is missing");


            var sessionUser = await _sessionService.ValidateSessionTokenAsync(token);
            var currentUserId = sessionUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            if (project.CreatorId != currentUserId) return Unauthorized();

            await _projectService.AddUsersToProjectAsync(projectId, userIds);

            var updatedMembers = project.Members.Select(m => new UserDto
            {
                Id = m.Id,
                Name = m.Name,
                UserName = m.UserName ?? "",
                Email = m.Email ?? "",
            }).ToList();

            return Ok(new { Success = true, Message = "Users added to added project successfully.", UpdatedMembers = updatedMembers });
        }

        [HttpPut("{projectId:guid}/remove-members")]
        [SessionAuthorize]
        public async Task<IActionResult> RemoveUsersFromProject(Guid projectId, [FromBody] List<string> userIds)
        {
         
            if (userIds == null || !userIds.Any())
            {
                return BadRequest("User IDs must be provided.");
            }



            var token = HttpContext.Request.Headers["SessionToken"].FirstOrDefault();

            if (token == null) throw new UnauthorizedAccessException("Session token is missing");


            var sessionUser = await _sessionService.ValidateSessionTokenAsync(token);
            var currentUserId = sessionUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            if (project.CreatorId != currentUserId) return Unauthorized();


            await _projectService.RemoveUsersFromProjectAsync(projectId, userIds);


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
        public async Task<IActionResult> InviteUserToProject([FromBody] ProjectInvitationDto dto)
        {
            try
            {
                var result = await _projectService.InviteUserToProjectAsync(dto);

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
