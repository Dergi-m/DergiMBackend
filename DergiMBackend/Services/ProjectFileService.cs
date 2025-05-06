using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using Microsoft.EntityFrameworkCore;


public class ProjectFileService : IProjectFileService
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectFileService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectFile> UploadFileAsync(ProjectFile file)
    {
        _dbContext.ProjectFiles.Add(file);
        await _dbContext.SaveChangesAsync();
        return file;
    }

    public async Task<ProjectFile?> UpdateFileAsync(UpdateProjectFileDto dto)
    {
        var file = await _dbContext.ProjectFiles.FirstOrDefaultAsync(f => f.Id == dto.Id);
        if (file == null) return null;

        file.FileUrl = dto.FileUrl;
        file.LocalFileUrl = dto.LocalFileUrl;
        file.UpdatedAt = DateTime.UtcNow;

        _dbContext.ProjectFiles.Update(file);
        await _dbContext.SaveChangesAsync();
        return file;
    }


    public async Task<IEnumerable<ProjectFile>> GetFilesForProjectAsync(Guid projectId)
    {
        return await _dbContext.ProjectFiles
            .Where(f => f.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<bool> DeleteFileAsync(Guid fileId)
    {
        var file = await _dbContext.ProjectFiles.FindAsync(fileId);
        if (file == null) return false;

        _dbContext.ProjectFiles.Remove(file);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
