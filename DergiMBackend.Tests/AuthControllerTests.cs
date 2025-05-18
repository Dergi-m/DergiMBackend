using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using DergiMBackend.Controllers;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;

namespace DergiMBackend.Tests;
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();
    private readonly Mock<ISessionService> _sessionService = new();
    private readonly Mock<IUserService> _userService = new();

    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(
            _authService.Object,
            _sessionService.Object,
            _userService.Object
        );
    }

    [Fact]
    public async Task GetAccessToken_ReturnsOk_WhenTokenValid()
    {
        // Arrange
        var request = new TokenRequestDto { ClientId = "valid", ClientSecret = "secret" };
        var token = new TokenResponseDto { AccessToken = "token123" };

        _authService
            .Setup(s => s.GetAccessTokenAsync(request))
            .ReturnsAsync(token);

        // Act
        var result = await _controller.GetAccessToken(request);

        // Assert
        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok.Value.Should().BeEquivalentTo(token);
    }

    [Fact]
    public async Task GetAccessToken_ReturnsUnauthorized_WhenTokenNull()
    {
        // Arrange
        var request = new TokenRequestDto { ClientId = "invalid", ClientSecret = "wrong" };

        _authService
            .Setup(s => s.GetAccessTokenAsync(request))
            .ReturnsAsync((TokenResponseDto?)null);

        // Act
        var result = await _controller.GetAccessToken(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetCurrentSession_ReturnsUnauthorized_WhenHeaderMissing()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetCurrentSession();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetCurrentSession_ReturnsUnauthorized_WhenClaimMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["SessionToken"] = "valid";

        var fakePrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // no claims
        _sessionService
            .Setup(s => s.ValidateSessionTokenAsync("valid"))
            .ReturnsAsync(fakePrincipal);

        _controller.ControllerContext.HttpContext = context;

        // Act
        var result = await _controller.GetCurrentSession();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetCurrentSession_ReturnsNotFound_WhenUserMissing()
    {
        // Arrange
        var userId = "user123";
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        var context = new DefaultHttpContext();
        context.Request.Headers["SessionToken"] = "valid";
        _controller.ControllerContext.HttpContext = context;

        _sessionService
            .Setup(s => s.ValidateSessionTokenAsync("valid"))
            .ReturnsAsync(principal);

        _userService
            .Setup(s => s.GetUserByIdAsync(userId))
            .Returns(Task.FromResult<UserDto>(null!));

        // Act
        var result = await _controller.GetCurrentSession();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCurrentSession_ReturnsOk_WhenSessionValid()
    {
        // Arrange
        var userId = "user123";
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        var user = new UserDto { Id = userId, Email = "test@mail.com", Name = "Test User", UserName = "testusertest"};

        var context = new DefaultHttpContext();
        context.Request.Headers["SessionToken"] = "valid";
        _controller.ControllerContext.HttpContext = context;

        _sessionService
            .Setup(s => s.ValidateSessionTokenAsync("valid"))
            .ReturnsAsync(principal);

        _userService
            .Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetCurrentSession();

        // Assert
        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
    }
}
