using Microsoft.AspNetCore.Http;

namespace DergiMBackend.Services.IServices
{
    public interface IBlobService
    {
        Task<string> UploadAsync(IFormFile file, Guid projectId);
        Task<string> UpdateAsync(string blobName, IFormFile file);
        Task<(byte[] Content, string ContentType)?> GetBlobAsync(string blobName);
        Task<bool> DeleteAsync(string blobName);
        string GetBlobUrl(string blobName);
    }
}
