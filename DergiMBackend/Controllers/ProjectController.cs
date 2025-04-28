using DergiMBackend.Authorization;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _projectService.CreateProjectAsync(createDto);
            var created = await _projectService.GetProjectByIdAsync(project.Id) ?? throw new Exception("Something went wrong please try again later");
            return Ok(created);
        }

        [HttpPut]
        [SessionAuthorize]
        public async Task<IActionResult> UpdateProject([FromBody] UpdateProjectDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedProject = await _projectService.UpdateProjectAsync(updateDto);

            if (updatedProject == null)
                return NotFound(new { message = "Project not found" });

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

            await _projectService.AddUsersToProjectAsync(projectId, userIds);

            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

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

            await _projectService.RemoveUsersFromProjectAsync(projectId, userIds);

            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            var updatedMembers = project.Members.Select(m => new UserDto
            {
                Id = m.Id,
                Name = m.Name,
                UserName = m.UserName ?? "",
                Email = m.Email ?? "",
            }).ToList();


            return Ok(new { Success = true, Message = "Users removed from the project successfully.", UpdatedMembers = updatedMembers });
        }


    }
}
