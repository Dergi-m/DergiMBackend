using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;

public interface IProjectFileService
{
    Task<ProjectFile> UploadFileAsync(ProjectFile file);
    Task<IEnumerable<ProjectFile>> GetFilesForProjectAsync(Guid projectId);
    Task<ProjectFile?> UpdateFileAsync(UpdateProjectFileDto dto);
    Task<ProjectFile?> DeleteFileAsync(Guid fileId);
}
