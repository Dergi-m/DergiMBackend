using DergiMBackend.Authorization;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services;
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
        private readonly IBlobService _blobService;

        public ProjectFileController(IProjectFileService projectFileService, IBlobService blobService)
        {
            _projectFileService = projectFileService;
            _blobService = blobService;

        }

        [HttpPost("upload/{projectId:guid}")]
        public async Task<IActionResult> UploadFileToBlob([FromForm] IFormFile file, Guid projectId)
        {
            if (file.Length == 0)
                return BadRequest("Invalid file.");

            var allowedTypes = new[]
            {
                "image/png",
                "image/jpeg",
                "image/jpg",
                "image/gif",
                "image/webp",
                "application/pdf",
                "application/msword", // .doc
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
                "application/vnd.ms-powerpoint", // .ppt
                "application/vnd.openxmlformats-officedocument.presentationml.presentation", // .pptx
                "application/vnd.ms-excel", // .xls
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" // .xlsx
            };
            
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Unsupported file type.");

            if (file.Length > 10000000)
            {
                return BadRequest("File size is too large. Upload file size should be less than 10MB.");
            }

            var fileId = Guid.NewGuid();
            var fileUrl = await _blobService.UploadAsync(file, fileId);

            var projectFile = new ProjectFile
            {
                Id = fileId,
                FileUrl = fileUrl,
                LocalFileUrl = file.FileName,
                ProjectId = projectId,
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

            var enrichedFiles = files.Select(file => new
            {
                file.Id,
                file.LocalFileUrl,
                file.ProjectId,
                file.CreatedAt,
                file.UpdatedAt,
                FileUrl = _blobService.GetBlobUrl(file.FileUrl)
            });

            return Ok(new { success = true, files = enrichedFiles });
        }


        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            var deletedFile = await _projectFileService.DeleteFileAsync(fileId);
            if (deletedFile == null)
                return NotFound(new { success = false, message = "File not found." });

            await _blobService.DeleteAsync(deletedFile.FileUrl);
            return Ok(new { success = true, message = "File deleted successfully." });
        }

        [HttpPut("{fileId}")]
        public async Task<IActionResult> UpdateFile([FromForm] IFormFile file, [FromForm] string blobName , Guid fileId)
        {
            var fileUrl = await _blobService.UpdateAsync(blobName, file);
            
            var newFile = new UpdateProjectFileDto
            {
                Id = fileId,
                FileUrl = fileUrl,
                LocalFileUrl = file.FileName
                
            };

            await _blobService.UpdateAsync(blobName, file);
            
            var updatedFile = await _projectFileService.UpdateFileAsync(newFile);
            
            if (updatedFile == null)
                return NotFound(new { success = false, message = "File not found." });

            return Ok(new { success = true, file = updatedFile });
        }

        [HttpGet("download/{blobName}")]
        public async Task<IActionResult> Download(string blobName, [FromQuery] string fileName)
        {
            var blobResult = await _blobService.GetBlobAsync(blobName);
            if (blobResult is null)
                return NotFound();
            
            var (content, contentType) = blobResult.Value;
            return File(content, contentType, fileName);
        }
    }
}
