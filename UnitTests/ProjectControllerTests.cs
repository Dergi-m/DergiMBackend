using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DergiMBackend.Controllers;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DergiMBackend.DbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace DergiMBackend.Tests.Controllers
{
    [TestClass]
    public class ProjectControllerTests
    {
        private ApplicationDbContext _dbContext;
        private Mock<IMapper> _mapperMock;
        private Mock<IConfiguration> _configurationMock;
        private ProjectController _projectController;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _configurationMock = new Mock<IConfiguration>();
            _projectController = new ProjectController(_dbContext, _mapperMock.Object, _configurationMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Projects.RemoveRange(_dbContext.Projects);
            _dbContext.ProjectFiles.RemoveRange(_dbContext.ProjectFiles);
            _dbContext.SaveChanges();
        }

        [TestMethod]
        public async Task Get_ReturnsAllProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, Name = "Project1", Description = "Description1", OrganisationId = 1 },
                new Project { Id = 2, Name = "Project2", Description = "Description2", OrganisationId = 2 }
            };
            await _dbContext.Projects.AddRangeAsync(projects);
            await _dbContext.SaveChangesAsync();

            var projectDtos = new List<ProjectDto>
            {
                new ProjectDto { Id = 1, Name = "Project1", Description = "Description1", OrganisationId = 1 },
                new ProjectDto { Id = 2, Name = "Project2", Description = "Description2", OrganisationId = 2 }
            };

			_mapperMock.Setup(m => m.Map<List<ProjectDto>>(It.IsAny<List<Project>>()))
	            .Returns(projectDtos);

			// Act
			var result = await _projectController.Get();

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(projectDtos, result.Result);
        }

        [TestMethod]
        public async Task Get_WithOrganisationId_ReturnsFilteredProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = 1, Name = "Project1", Description = "Description1", OrganisationId = 1 },
                new Project { Id = 2, Name = "Project2", Description = "Description2", OrganisationId = 2 }
            };
            await _dbContext.Projects.AddRangeAsync(projects);
            await _dbContext.SaveChangesAsync();

            var filteredProjects = new List<Project> { projects[0] };
            var projectDtos = new List<ProjectDto>
            {
                new ProjectDto { Id = 1, Name = "Project1", Description = "Description1", OrganisationId = 1 }
            };

			_mapperMock.Setup(m => m.Map<List<ProjectDto>>(It.IsAny<List<Project>>()))
	  .Returns(projectDtos);

			// Act
			var result = await _projectController.Get(organisationId:1);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(projectDtos, result.Result);
        }
		

        [TestMethod]
        public async Task Get_WithProjectId_ReturnsProject()
        {
            // Arrange
            var project = new Project { Id = 1, Name = "Project1", Description = "Description1", OrganisationId = 1 };
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            var projectDto = new ProjectDto { Id = 1, Name = "Project1", Description = "Description1", OrganisationId = 1 };
            _mapperMock.Setup(m => m.Map<ProjectDto>(project)).Returns(projectDto);

            // Act
            var result = await _projectController.Get(1);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(projectDto, result.Result);
        }

        [TestMethod]
        public async Task Create_AddsProject()
        {
            // Arrange
            var projectDto = new ProjectDto { Name = "Project1", Description = "Description1", OrganisationId = 1 };
            var project = new Project { Name = "Project1", Description = "Description1", OrganisationId = 1 };

            _mapperMock.Setup(m => m.Map<Project>(projectDto)).Returns(project);
            _mapperMock.Setup(m => m.Map<ProjectDto>(project)).Returns(projectDto);

            // Act
            var result = await _projectController.Create(projectDto);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(projectDto, result.Result);
            Assert.AreEqual(1, await _dbContext.Projects.CountAsync());
        }

        [TestMethod]
        public async Task Update_UpdatesProject()
        {
            // Arrange
            var project = new Project { Id = 1, Name = "Project1", Description = "Old Description", OrganisationId = 1 };
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            var projectDto = new ProjectDto { Id = 1, Name = "UpdatedProject", Description = "Updated Description", OrganisationId = 1 };
            _mapperMock.Setup(m => m.Map<ProjectDto>(project)).Returns(projectDto);

            // Act
            var result = await _projectController.Update(projectDto);

            // Assert
            Assert.IsTrue(result.Success);
            var updatedProject = await _dbContext.Projects.FindAsync(1);
            Assert.AreEqual("UpdatedProject", updatedProject.Name);
            Assert.AreEqual("Updated Description", updatedProject.Description);
        }

        [TestMethod]
        public async Task Delete_RemovesProject()
        {
            // Arrange
            var project = new Project { Id = 1, Name = "Project1", Description = "Description1", OrganisationId = 1 };
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _projectController.Delete(1);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, await _dbContext.Projects.CountAsync());
        }

        [TestMethod]
        public async Task AddFile_AddsProjectFile()
        {
            // Arrange
            var projectFileDto = new ProjectFileDto
            {
                ProjectId = 1,
                File = new FormFile(new MemoryStream(), 0, 0, "Data", "test.pdf")
            };

            _configurationMock.Setup(c => c["ApiSettings:BaseUrl"]).Returns("http://localhost");

            // Act
            var result = await _projectController.AddFile(projectFileDto);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, await _dbContext.ProjectFiles.CountAsync());
        }

        [TestMethod]
        public async Task DeleteFile_RemovesProjectFile()
        {
            // Arrange
            var projectFile = new ProjectFile { Id = 1, FileUrl = "http://localhost/test.txt", LocalFileUrl = "test.txt", ProjectId = 1 };
            await _dbContext.ProjectFiles.AddAsync(projectFile);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _projectController.DeleteFile(1);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, await _dbContext.ProjectFiles.CountAsync());
        }
    }
}
