using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using DergiMBackend.Controllers;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
namespace DergiMBackend.Tests;

public class OrganisationControllerTests
{
    private readonly Mock<IOrganisationService> _organisationService = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly OrganisationController _controller;

    public OrganisationControllerTests()
    {
        _controller = new OrganisationController(_organisationService.Object, _userService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithOrganisations()
    {
        var expected = new List<Organisation>
        {
            new() { Id = Guid.NewGuid(), Name = "Org1", UniqueName = "org1" }
        };

        _organisationService
            .Setup(s => s.GetAllOrganisationsAsync())
            .ReturnsAsync(expected);

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenOrganisationExists()
    {
        var id = Guid.NewGuid();
        var org = new Organisation { Id = id, Name = "Test", UniqueName = "test" };

        _organisationService
            .Setup(s => s.GetOrganisationByIdAsync(id))
            .ReturnsAsync(org);

        var result = await _controller.Get(id);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(org);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenOrganisationMissing()
    {
        var id = Guid.NewGuid();

        _organisationService
            .Setup(s => s.GetOrganisationByIdAsync(id))
            .ReturnsAsync((Organisation?)null);

        var result = await _controller.Get(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("Name", "required");

        var dto = new CreateOrganisationDto();

        var result = await _controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenOwnerNotFound()
    {
        var dto = new CreateOrganisationDto
        {
            OwnerId = Guid.NewGuid(),
            Name = "Org",
            UniqueName = "org",
            Description = "desc"
        };

        _userService
            .Setup(s => s.GetUserEntityByIdAsync(dto.OwnerId.ToString()))
            .Returns(Task.FromResult<ApplicationUser>(null!));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenSuccessful()
    {
        var dto = new CreateOrganisationDto
        {
            OwnerId = Guid.NewGuid(),
            Name = "Org",
            UniqueName = "org",
            Description = "desc"
        };

        var user = new ApplicationUser{ Id = dto.OwnerId.ToString(), Email = "user@example.com", UserName = "testusertest", Name = "test user"};
        var created = new Organisation { Id = Guid.NewGuid(), Name = dto.Name, UniqueName = dto.UniqueName };

        _userService
            .Setup(s => s.GetUserEntityByIdAsync(dto.OwnerId.ToString()))
            .ReturnsAsync(user);

        _organisationService
            .Setup(s => s.CreateOrganisationAsync(dto.UniqueName, dto.Name, dto.Description, user))
            .ReturnsAsync(created);

        var result = await _controller.Create(dto);

        result.Should().BeOfType<CreatedAtActionResult>()
              .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccessful()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateOrganisationDto { Name = "Updated", Description = "New Desc" };

        var updated = new Organisation { Id = id, Name = dto.Name, Description = dto.Description, UniqueName = "org" };

        _organisationService
            .Setup(s => s.UpdateOrganisationAsync(id, dto.Name, dto.Description))
            .ReturnsAsync(updated);

        var result = await _controller.Update(id, dto);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenOrganisationMissing()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateOrganisationDto { Name = "Updated", Description = "New Desc" };

        _organisationService
            .Setup(s => s.UpdateOrganisationAsync(id, dto.Name, dto.Description))
            .ReturnsAsync((Organisation?)null);

        var result = await _controller.Update(id, dto);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        var id = Guid.NewGuid();

        _organisationService
            .Setup(s => s.DeleteOrganisationAsync(id))
            .ReturnsAsync(true);

        var result = await _controller.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenNotDeleted()
    {
        var id = Guid.NewGuid();

        _organisationService
            .Setup(s => s.DeleteOrganisationAsync(id))
            .ReturnsAsync(false);

        var result = await _controller.Delete(id);

        result.Should().BeOfType<NotFoundResult>();
    }
}
