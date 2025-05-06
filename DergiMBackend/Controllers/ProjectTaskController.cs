using DergiMBackend.Authorization;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SessionAuthorize]
    public class ProjectTaskController : ControllerBase
    {
        private readonly IProjectTaskService _taskService;

        public ProjectTaskController(IProjectTaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateProjectTaskDto dto)
        {
            var task = await _taskService.CreateProjectTaskAsync(dto);
            return Ok(task);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateProjectTaskDto dto)
        {
            var updated = await _taskService.UpdateProjectTaskAsync(dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            var deleted = await _taskService.DeleteProjectTaskAsync(taskId);
            return deleted ? Ok() : NotFound();
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            var task = await _taskService.GetProjectTaskByIdAsync(taskId);
            return task == null ? NotFound() : Ok(task);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(Guid projectId)
        {
            var tasks = await _taskService.GetTasksByProjectIdAsync(projectId);
            return Ok(tasks);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetTasksForUser(string userId)
        {
            var tasks = await _taskService.GetTasksForUserAsync(userId);
            return Ok(tasks);
        }
    }
}
