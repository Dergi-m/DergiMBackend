using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DergiMBackend.Controllers;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
namespace DergiMBackend.Tests;

public class ProjectFileControllerTests
{
    private readonly Mock<IProjectFileService> _projectFileService = new();
    private readonly Mock<IBlobService> _blobService = new();
    private readonly ProjectFileController _controller;

    public ProjectFileControllerTests()
    {
        _controller = new ProjectFileController(_projectFileService.Object, _blobService.Object);
    }

    private static IFormFile CreateMockFile(string fileName, string contentType, int size = 1000)
    {
        var fileMock = new Mock<IFormFile>();
        var content = new MemoryStream(new byte[size]);
        fileMock.Setup(f => f.Length).Returns(size);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.OpenReadStream()).Returns(content);
        return fileMock.Object;
    }

    [Fact]
    public async Task UploadFileToBlob_ReturnsBadRequest_WhenEmpty()
    {
        var file = CreateMockFile("test.pdf", "application/pdf", 0);
        var result = await _controller.UploadFileToBlob(file, Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadFileToBlob_ReturnsBadRequest_WhenFileTypeInvalid()
    {
        var file = CreateMockFile("test.exe", "application/x-msdownload");
        var result = await _controller.UploadFileToBlob(file, Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>()
              .Which.Value.Should().Be("Unsupported file type.");
    }

    [Fact]
    public async Task UploadFileToBlob_ReturnsOk_WhenSuccessful()
    {
        var file = CreateMockFile("test.pdf", "application/pdf");
        var projectId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var fileUrl = "https://fakeurl.com/file.pdf";
        var savedFile = new ProjectFile { Id = fileId, FileUrl = fileUrl };

        _blobService.Setup(s => s.UploadAsync(file, It.IsAny<Guid>())).ReturnsAsync(fileUrl);
        _projectFileService.Setup(s => s.UploadFileAsync(It.IsAny<ProjectFile>())).ReturnsAsync(savedFile);

        var result = await _controller.UploadFileToBlob(file, projectId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetFiles_ReturnsFiles_WithBlobUrls()
    {
        var projectId = Guid.NewGuid();
        var rawFiles = new List<ProjectFile>
        {
            new() { Id = Guid.NewGuid(), FileUrl = "abc.jpg", LocalFileUrl = "abc.jpg", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _projectFileService.Setup(s => s.GetFilesForProjectAsync(projectId)).ReturnsAsync(rawFiles);
        _blobService.Setup(s => s.GetBlobUrl("abc.jpg")).Returns("https://blob.com/abc.jpg");

        var result = await _controller.GetFiles(projectId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteFile_ReturnsNotFound_WhenMissing()
    {
        var id = Guid.NewGuid();

        _projectFileService.Setup(s => s.DeleteFileAsync(id)).ReturnsAsync((ProjectFile?)null);

        var result = await _controller.DeleteFile(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteFile_ReturnsOk_WhenDeleted()
    {
        var id = Guid.NewGuid();
        var file = new ProjectFile { FileUrl = "abc.pdf" };


        _projectFileService
            .Setup(s => s.DeleteFileAsync(It.IsAny<Guid>()))
            .ReturnsAsync(file);

        _blobService
            .Setup(s => s.DeleteAsync(file.FileUrl))
            .Returns(Task.FromResult(true));
        
        var result = await _controller.DeleteFile(id);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateFile_ReturnsNotFound_WhenMissing()
    {
        var file = CreateMockFile("new.pdf", "application/pdf");
        var blobName = "oldname.pdf";
        var fileId = Guid.NewGuid();

        _blobService.Setup(s => s.UpdateAsync(blobName, file)).ReturnsAsync("new-url.pdf");
        _projectFileService.Setup(s => s.UpdateFileAsync(It.IsAny<UpdateProjectFileDto>())).ReturnsAsync((ProjectFile?)null);

        var result = await _controller.UpdateFile(file, blobName, fileId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateFile_ReturnsOk_WhenUpdated()
    {
        var file = CreateMockFile("new.pdf", "application/pdf");
        var blobName = "oldname.pdf";
        var fileId = Guid.NewGuid();
        var updated = new ProjectFile { Id = fileId, FileUrl = "new-url.pdf" };

        _blobService.Setup(s => s.UpdateAsync(blobName, file)).ReturnsAsync("new-url.pdf");
        _projectFileService.Setup(s => s.UpdateFileAsync(It.IsAny<UpdateProjectFileDto>())).ReturnsAsync(updated);

        var result = await _controller.UpdateFile(file, blobName, fileId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Download_ReturnsFile_WhenExists()
    {
        var blobName = "abc.png";
        var fileName = "logo.png";
        var content = new byte[10];
        var contentType = "image/png";

        _blobService.Setup(s => s.GetBlobAsync(blobName)).ReturnsAsync((content, contentType));

        var result = await _controller.Download(blobName, fileName);

        result.Should().BeOfType<FileContentResult>()
              .Which.ContentType.Should().Be(contentType);
    }

    [Fact]
    public async Task Download_ReturnsNotFound_WhenMissing()
    {
        _blobService.Setup(s => s.GetBlobAsync("missing")).ReturnsAsync((ValueTuple<byte[], string>?)null);

        var result = await _controller.Download("missing", "file.txt");

        result.Should().BeOfType<NotFoundResult>();
    }
}
