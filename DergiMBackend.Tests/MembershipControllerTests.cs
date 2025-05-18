using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using DergiMBackend.Controllers;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;

namespace DergiMBackend.Tests;

public class MembershipControllerTests
{
    private readonly Mock<IOrganisationMembershipService> _membershipServiceMock = new();
    private readonly MembershipController _controller;

    public MembershipControllerTests()
    {
        _controller = new MembershipController(_membershipServiceMock.Object);
    }

    [Fact]
    public async Task GetMembershipsForOrganisation_ReturnsOk_WithMemberships()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var expected = new List<OrganisationMembership>
        {
            new() { Id = Guid.NewGuid(), UserId = "u1", OrganisationId = organisationId, RoleId = Guid.NewGuid() }
        };

        _membershipServiceMock
            .Setup(s => s.GetMembershipsForOrganisationAsync(organisationId))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetMembershipsForOrganisation(organisationId);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetMembershipsForUser_ReturnsOk_WithMemberships()
    {
        var userId = "user123";
        var expected = new List<OrganisationMembership>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, OrganisationId = Guid.NewGuid(), RoleId = Guid.NewGuid() }
        };

        _membershipServiceMock
            .Setup(s => s.GetMembershipsForUserAsync(userId))
            .ReturnsAsync(expected);

        var result = await _controller.GetMembershipsForUser(userId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetMembershipById_ReturnsOk_WhenFound()
    {
        var id = Guid.NewGuid();
        var expected = new OrganisationMembership { Id = id, UserId = "u", OrganisationId = Guid.NewGuid(), RoleId = Guid.NewGuid() };

        _membershipServiceMock
            .Setup(s => s.GetMembershipByIdAsync(id))
            .ReturnsAsync(expected);

        var result = await _controller.GetMembershipById(id);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetMembershipById_ReturnsNotFound_WhenNull()
    {
        var id = Guid.NewGuid();

        _membershipServiceMock
            .Setup(s => s.GetMembershipByIdAsync(id))
            .ReturnsAsync((OrganisationMembership?)null);

        var result = await _controller.GetMembershipById(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateMembership_ReturnsCreated_WhenValid()
    {
        var dto = new CreateMembershipDto
        {
            UserId = "user1",
            OrganisationId = Guid.NewGuid(),
            RoleId = Guid.NewGuid()
        };

        var created = new OrganisationMembership
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            OrganisationId = dto.OrganisationId,
            RoleId = dto.RoleId
        };

        _membershipServiceMock
            .Setup(s => s.CreateMembershipAsync(It.IsAny<OrganisationMembership>()))
            .ReturnsAsync(created);

        var result = await _controller.CreateMembership(dto);

        result.Should().BeOfType<CreatedAtActionResult>()
              .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task CreateMembership_ReturnsBadRequest_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("test", "invalid");

        var dto = new CreateMembershipDto(); // invalid on purpose

        var result = await _controller.CreateMembership(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteMembership_ReturnsNoContent_WhenDeleted()
    {
        var id = Guid.NewGuid();

        _membershipServiceMock
            .Setup(s => s.DeleteMembershipAsync(id))
            .ReturnsAsync(true);

        var result = await _controller.DeleteMembership(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteMembership_ReturnsNotFound_WhenNotDeleted()
    {
        var id = Guid.NewGuid();

        _membershipServiceMock
            .Setup(s => s.DeleteMembershipAsync(id))
            .ReturnsAsync(false);

        var result = await _controller.DeleteMembership(id);

        result.Should().BeOfType<NotFoundResult>();
    }
}
