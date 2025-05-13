using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DergiMBackend.Controllers;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;

public class ProjectTaskControllerTests
{
    private readonly Mock<IProjectTaskService> _taskService = new();
    private readonly ProjectTaskController _controller;

    public ProjectTaskControllerTests()
    {
        _controller = new ProjectTaskController(_taskService.Object);
    }

    [Fact]
    public async Task CreateTask_ReturnsOk_WhenCreated()
    {
        var dto = new CreateProjectTaskDto { Name = "Task" };
        var created = new ProjectTask { Id = Guid.NewGuid(), Name = "Task" };

        _taskService.Setup(s => s.CreateProjectTaskAsync(dto)).ReturnsAsync(created);

        var result = await _controller.CreateTask(dto);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task UpdateTask_ReturnsOk_WhenUpdated()
    {
        var dto = new UpdateProjectTaskDto { Id = Guid.NewGuid(), Name = "Updated" };
        var updated = new ProjectTask { Id = dto.Id, Name = dto.Name };

        _taskService.Setup(s => s.UpdateProjectTaskAsync(dto)).ReturnsAsync(updated);

        var result = await _controller.UpdateTask(dto);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNotFound_WhenNull()
    {
        var dto = new UpdateProjectTaskDto { Id = Guid.NewGuid(), Name = "Fail" };

        _taskService.Setup(s => s.UpdateProjectTaskAsync(dto)).ReturnsAsync((ProjectTask?)null);

        var result = await _controller.UpdateTask(dto);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteTask_ReturnsOk_WhenDeleted()
    {
        var taskId = Guid.NewGuid();

        _taskService.Setup(s => s.DeleteProjectTaskAsync(taskId)).ReturnsAsync(true);

        var result = await _controller.DeleteTask(taskId);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenFalse()
    {
        var taskId = Guid.NewGuid();

        _taskService.Setup(s => s.DeleteProjectTaskAsync(taskId)).ReturnsAsync(false);

        var result = await _controller.DeleteTask(taskId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetTaskById_ReturnsOk_WhenFound()
    {
        var taskId = Guid.NewGuid();
        var task = new ProjectTask { Id = taskId, Name = "Task" };

        _taskService.Setup(s => s.GetProjectTaskByIdAsync(taskId)).ReturnsAsync(task);

        var result = await _controller.GetTaskById(taskId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(task);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenMissing()
    {
        var taskId = Guid.NewGuid();

        _taskService.Setup(s => s.GetProjectTaskByIdAsync(taskId)).ReturnsAsync((ProjectTask?)null);

        var result = await _controller.GetTaskById(taskId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetTasksByProject_ReturnsOk_WithList()
    {
        var projectId = Guid.NewGuid();
        var list = new List<ProjectTask> { new() { Name = "Task1" } };

        _taskService.Setup(s => s.GetTasksByProjectIdAsync(projectId)).ReturnsAsync(list);

        var result = await _controller.GetTasksByProject(projectId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetTasksForUser_ReturnsOk_WithList()
    {
        var userId = "user123";
        var list = new List<ProjectTask> { new() { Name = "TaskA" } };

        _taskService.Setup(s => s.GetTasksForUserAsync(userId)).ReturnsAsync(list);

        var result = await _controller.GetTasksForUser(userId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(list);
    }
}
