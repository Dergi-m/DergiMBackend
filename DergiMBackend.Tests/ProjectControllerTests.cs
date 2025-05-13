using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DergiMBackend.Controllers;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;

public class ProjectControllerTests
{
    private readonly Mock<IProjectService> _projectService = new();
    private readonly Mock<ISessionService> _sessionService = new();
    private readonly Mock<IMapper> _mapper = new();

    private readonly ProjectController _controller;

    public ProjectControllerTests()
    {
        _controller = new ProjectController(
            _projectService.Object,
            _sessionService.Object,
            _mapper.Object
        );

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private void AddSessionTokenToHeader(string token)
    {
        _controller.ControllerContext.HttpContext.Request.Headers["SessionToken"] = token;
    }

    private ClaimsPrincipal CreateMockUser(string userId)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "mock");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task GetProject_ReturnsOk_WhenFound()
    {
        var projectId = Guid.NewGuid();
        var creator = new UserDto {Name = "creator", Id = "creator-id", UserName = "creator123", Email = "creator@test.com"};
        var project = new Project { Id = projectId, Name = "Test" };
        var dto = new ProjectDetailsDto { Id = projectId, Name = "Test", CreatorId = creator.Id};

        _projectService.Setup(s => s.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        _mapper.Setup(m => m.Map<ProjectDetailsDto>(project)).Returns(dto);

        var result = await _controller.GetProject(projectId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetProject_ReturnsNotFound_WhenMissing()
    {
        var projectId = Guid.NewGuid();
        _projectService.Setup(s => s.GetProjectByIdAsync(projectId)).ReturnsAsync((Project?)null);

        var result = await _controller.GetProject(projectId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetProjectsForOrganisation_ReturnsOk()
    {
        var organisationId = Guid.NewGuid();
        var list = new List<Project> { new Project { Id = Guid.NewGuid(), Name = "test-project"} };

        _projectService.Setup(s => s.GetProjectsForOrganisationAsync(organisationId)).ReturnsAsync(list);

        var result = await _controller.GetProjectsForOrganisation(organisationId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task CreateProject_ReturnsOk_WhenValid()
    {
        var token = "valid-token";
        var userId = "user-123";
        var project = new Project { Id = Guid.NewGuid(), CreatorId = userId, Name = "test-project"};

        AddSessionTokenToHeader(token);
        _sessionService.Setup(s => s.ValidateSessionTokenAsync(token)).ReturnsAsync(CreateMockUser(userId));
        _projectService.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectDto>(), userId)).ReturnsAsync(project);
        _projectService.Setup(s => s.GetProjectByIdAsync(project.Id)).ReturnsAsync(project);

        var createDto = new CreateProjectDto { Name = "New Project" };

        var result = await _controller.CreateProject(createDto);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(project);
    }

    [Fact]
    public async Task UpdateProject_ReturnsUnauthorized_WhenNotCreator()
    {
        var projectId = Guid.NewGuid();
        var token = "token";
        var updateDto = new UpdateProjectDto { Id = projectId, Name = "Updated" };

        AddSessionTokenToHeader(token);
        _sessionService.Setup(s => s.ValidateSessionTokenAsync(token)).ReturnsAsync(CreateMockUser("user-123"));

        _projectService.Setup(s => s.GetProjectByIdAsync(projectId)).ReturnsAsync(new Project
        {
            Id = projectId,
            CreatorId = "other-user",
            Name = "test-project"
        });

        var result = await _controller.UpdateProject(updateDto);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task DeleteProject_ReturnsOk_WhenDeleted()
    {
        var projectId = Guid.NewGuid();

        _projectService.Setup(s => s.DeleteProjectAsync(projectId)).ReturnsAsync(true);

        var result = await _controller.DeleteProject(projectId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteProject_ReturnsNotFound_WhenMissing()
    {
        var projectId = Guid.NewGuid();

        _projectService.Setup(s => s.DeleteProjectAsync(projectId)).ReturnsAsync(false);

        var result = await _controller.DeleteProject(projectId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task InviteUserToProject_ReturnsUnauthorized_WhenNotCreator()
    {
        var projectId = Guid.NewGuid();
        var token = "token";
        var creatorId = "real-creator";
        var fakeUserId = "not-creator";

        AddSessionTokenToHeader(token);
        _sessionService.Setup(s => s.ValidateSessionTokenAsync(token)).ReturnsAsync(CreateMockUser(fakeUserId));
        _projectService.Setup(s => s.GetProjectByIdAsync(projectId)).ReturnsAsync(new Project { Id = projectId, CreatorId = creatorId, Name = "test-project"});

        var dto = new CreateProjectInvitationDto
        {
            ProjectId = projectId,
            Message = "join us",
            TargetUserId = "target-user"
        };

        var result = await _controller.InviteUserToProject(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task InviteUserToProject_ReturnsOk_WhenValid()
    {
        var projectId = Guid.NewGuid();
        var token = "token";
        var creatorId = "creator-1";

        AddSessionTokenToHeader(token);
        _sessionService.Setup(s => s.ValidateSessionTokenAsync(token)).ReturnsAsync(CreateMockUser(creatorId));
        _projectService.Setup(s => s.GetProjectByIdAsync(projectId)).ReturnsAsync(new Project { Id = projectId, CreatorId = creatorId, Name = "test-project"});
        _projectService.Setup(s => s.InviteUserToProjectAsync(It.IsAny<ProjectInvitationDto>())).ReturnsAsync(true);

        var dto = new CreateProjectInvitationDto
        {
            ProjectId = projectId,
            Message = "join us",
            TargetUserId = "target-user"
        };

        var result = await _controller.InviteUserToProject(dto);

        result.Should().BeOfType<OkObjectResult>();
    }
}
