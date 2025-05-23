﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DergiMBackend.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlob:ConnectionString"];
            var containerName = configuration["AzureBlob:ContainerName"];
            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadAsync(IFormFile file, Guid fileId)
        {
            var blobClient = _containerClient.GetBlobClient(fileId + Path.GetExtension(file.FileName));
            
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blobClient.Name;
        }

        public async Task<(byte[] Content, string ContentType)?> GetBlobAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            if (!await blobClient.ExistsAsync()) return null;

            var download = await blobClient.DownloadAsync();
            using var ms = new MemoryStream();
            await download.Value.Content.CopyToAsync(ms);
            return (ms.ToArray(), download.Value.Details.ContentType);
        }
        
        public async Task<string> UpdateAsync(string blobName, IFormFile file)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            
            if (!await blobClient.ExistsAsync())
            {
                throw new DirectoryNotFoundException("File not found for update");
            }
            
            
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(
                    stream,
                    new BlobHttpHeaders { ContentType = file.ContentType }
                );
            }
            
            return blobClient.Name;
        }

        public async Task<bool> DeleteAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            return await blobClient.DeleteIfExistsAsync();
        }

        public string GetBlobUrl(string blobName)
        {
            return _containerClient.GetBlobClient(blobName).Uri.ToString();
        }
    }
}
