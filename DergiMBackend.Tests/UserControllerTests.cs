using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DergiMBackend.Controllers;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(_userService.Object);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsValid()
    {
        var login = new LoginRequestDto { UserName = "user", Password = "pass" };
        var userDto = new UserDto { UserName = login.UserName, Name = login.UserName, Id = Guid.NewGuid().ToString(), Email = "test@test.com"};
        var session = new SessionDto { SessionToken = "abc", User = userDto};

        _userService.Setup(s => s.LoginAsync(login)).ReturnsAsync(session);

        var result = await _controller.Login(login);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(session);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenExceptionThrown()
    {
        var login = new LoginRequestDto { UserName = "user", Password = "wrong" };

        _userService.Setup(s => s.LoginAsync(login)).ThrowsAsync(new Exception("Invalid"));

        var result = await _controller.Login(login);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenNewUser()
    {
        var register = new RegistrationRequestDto { UserName = "new", Email = "test@mail.com", Name = "new", Password = "123456789As-"};
        var userDto = new UserDto { UserName = register.UserName, Name = register.UserName, Id = Guid.NewGuid().ToString(), Email = register.Email};
        var session = new SessionDto { SessionToken = "xyz", User = userDto};

        _userService.Setup(s => s.IsUserUniqueAsync(register.UserName, register.Email)).ReturnsAsync(true);
        _userService.Setup(s => s.RegisterAsync(register)).ReturnsAsync(session);

        var result = await _controller.Register(register);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(session);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUsernameExists()
    {
        var register = new RegistrationRequestDto { UserName = "taken", Email = "test@mail.com", Name = "taken", Password = "123456789As-"};

        _userService.Setup(s => s.IsUserUniqueAsync(register.UserName, register.Email)).ReturnsAsync(false);

        var result = await _controller.Register(register);

        result.Should().BeOfType<BadRequestObjectResult>()
              .Which.Value.Should().BeEquivalentTo(new { error = "Username already exists" });
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenExceptionThrown()
    {
        var register = new RegistrationRequestDto { UserName = "fail", Email = "test@mail.com", Name = "fail", Password = "123456789As-"};

        _userService.Setup(s => s.IsUserUniqueAsync(register.UserName, register.Email)).ReturnsAsync(true);
        _userService.Setup(s => s.RegisterAsync(register)).ThrowsAsync(new Exception("fail"));

        var result = await _controller.Register(register);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetUser_ReturnsOk_WhenFound()
    {
        var username = "tester";
        var user = new UserDto { UserName = username, Name = username, Id = Guid.NewGuid().ToString(), Email = "test@test.com"};

        _userService.Setup(s => s.GetUserAsync(username)).ReturnsAsync(user);

        var result = await _controller.GetUser(username);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenMissing()
    {
        var username = "unknown";

        _userService.Setup(s => s.GetUserAsync(username)).ReturnsAsync((UserDto?)null);

        var result = await _controller.GetUser(username);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserById_ReturnsOk_WhenFound()
    {
        var userId = "abc";
        var user = new UserDto { Id = userId, Name = "asd", UserName = "asdsada", Email = "test@test.com"};

        _userService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);

        var result = await _controller.GetUserById(userId);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenMissing()
    {
        var userId = "notfound";

        _userService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((UserDto?)null);

        var result = await _controller.GetUserById(userId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOk_WithUsers()
    {
        var users = new List<UserDto>
        {
            new() { UserName = "test1", Name = "test1", Id = Guid.NewGuid().ToString(), Email = "test1@test.com"},
            new() { UserName = "test2", Name = "test2", Id = Guid.NewGuid().ToString(), Email = "test2@test.com" }
        };

        _userService.Setup(s => s.GetUsersAsync()).ReturnsAsync(users);

        var result = await _controller.GetAllUsers();

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(users);
    }
}
