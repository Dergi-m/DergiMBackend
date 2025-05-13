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

public class OrganisationRoleControllerTests
{
    private readonly Mock<IOrganisationRoleService> _roleService = new();
    private readonly OrganisationRoleController _controller;

    public OrganisationRoleControllerTests()
    {
        _controller = new OrganisationRoleController(_roleService.Object);
    }

    [Fact]
    public async Task GetRoles_ReturnsOk_WithRoleList()
    {
        var organisationId = Guid.NewGuid();
        var expected = new List<OrganisationRole>
        {
            new() { Id = Guid.NewGuid(), Name = "Editor", OrganisationId = organisationId }
        };

        _roleService
            .Setup(s => s.GetRolesForOrganisationAsync(organisationId))
            .ReturnsAsync(expected);

        var result = await _controller.GetRoles(organisationId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetRole_ReturnsOk_WhenFound()
    {
        var roleId = Guid.NewGuid();
        var role = new OrganisationRole { Id = roleId, Name = "Admin" };

        _roleService
            .Setup(s => s.GetRoleByIdAsync(roleId))
            .ReturnsAsync(role);

        var result = await _controller.GetRole(roleId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(role);
    }

    [Fact]
    public async Task GetRole_ReturnsNotFound_WhenNull()
    {
        var roleId = Guid.NewGuid();

        _roleService
            .Setup(s => s.GetRoleByIdAsync(roleId))
            .ReturnsAsync((OrganisationRole?)null);

        var result = await _controller.GetRole(roleId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");

        var result = await _controller.CreateRole(Guid.NewGuid(), new CreateOrganisationRoleDto());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateRole_ReturnsCreated_WhenSuccessful()
    {
        var organisationId = Guid.NewGuid();
        var dto = new CreateOrganisationRoleDto
        {
            Name = "Editor",
            Description = "Can edit content",
            CanAssignTasks = true,
            CanCreateTasks = true,
            VisibleTags = ["news", "tech"]
        };

        var created = new OrganisationRole
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            CanAssignTasks = dto.CanAssignTasks,
            CanCreateTasks = dto.CanCreateTasks,
            VisibleTags = dto.VisibleTags,
            OrganisationId = organisationId
        };

        _roleService
            .Setup(s => s.CreateRoleAsync(organisationId, It.IsAny<OrganisationRole>()))
            .ReturnsAsync(created);

        var result = await _controller.CreateRole(organisationId, dto);

        result.Should().BeOfType<CreatedAtActionResult>()
              .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task UpdateRole_ReturnsBadRequest_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var createDto = new CreateOrganisationRoleDto
        {
            Name = "test",
            CanAssignTasks = true,
            CanCreateTasks = true,
            VisibleTags = ["news", "tech"],
            Description = "test"
        };
        

        var result = await _controller.UpdateRole(Guid.NewGuid(), new UpdateOrganisationRoleDto{Name = "Name", Id = Guid.Empty});

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateRole_ReturnsOk_WhenSuccessful()
    {
        var roleId = Guid.NewGuid();
        var dto = new UpdateOrganisationRoleDto
        {
            Id = roleId,
            Name = "Updated",
            Description = "Updated role",
            CanAssignTasks = false,
            CanCreateTasks = true,
            VisibleTags = ["general"]
        };

        var updated = new OrganisationRole
        {
            Id = roleId,
            Name = dto.Name,
            Description = dto.Description,
            CanAssignTasks = dto.CanAssignTasks,
            CanCreateTasks = dto.CanCreateTasks,
            VisibleTags = dto.VisibleTags
        };

        _roleService
            .Setup(s => s.UpdateRoleAsync(It.IsAny<OrganisationRole>()))
            .ReturnsAsync(updated);

        var result = await _controller.UpdateRole(roleId, dto);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task DeleteRole_ReturnsNoContent_WhenDeleted()
    {
        var roleId = Guid.NewGuid();

        _roleService
            .Setup(s => s.DeleteRoleAsync(roleId))
            .ReturnsAsync(true);

        var result = await _controller.DeleteRole(roleId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteRole_ReturnsNotFound_WhenNotDeleted()
    {
        var roleId = Guid.NewGuid();

        _roleService
            .Setup(s => s.DeleteRoleAsync(roleId))
            .ReturnsAsync(false);

        var result = await _controller.DeleteRole(roleId);

        result.Should().BeOfType<NotFoundResult>();
    }
}
