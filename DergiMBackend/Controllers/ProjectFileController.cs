using DergiMBackend.Authorization;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SessionAuthorize]
    public class ProjectFileController : ControllerBase
    {
        private readonly IProjectFileService _projectFileService;

        public ProjectFileController(IProjectFileService projectFileService)
        {
            _projectFileService = projectFileService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile([FromBody] CreateProjectFileDto dto)
        {
            var projectFile = new ProjectFile
            {
                Id = Guid.NewGuid(),
                FileUrl = dto.FileUrl,
                LocalFileUrl = dto.LocalFileUrl,
                ProjectId = dto.ProjectId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _projectFileService.UploadFileAsync(projectFile);
            return Ok(new { success = true, file = result });
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetFiles(Guid projectId)
        {
            var files = await _projectFileService.GetFilesForProjectAsync(projectId);
            return Ok(files);
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            var result = await _projectFileService.DeleteFileAsync(fileId);
            if (!result)
                return NotFound(new { success = false, message = "File not found." });

            return Ok(new { success = true, message = "File deleted successfully." });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateFile([FromBody] UpdateProjectFileDto dto)
        {
            var updatedFile = await _projectFileService.UpdateFileAsync(dto);
            if (updatedFile == null)
                return NotFound(new { success = false, message = "File not found." });

            return Ok(new { success = true, file = updatedFile });
        }

    }
}
