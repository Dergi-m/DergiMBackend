using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;

        public UserController(IUserService userService, ISessionService sessionService)
        {
            _userService = userService;
            _sessionService = sessionService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<SessionDto>> Login([FromBody] LoginRequestDto request)
        {
            var result = await _userService.LoginAsync(request);
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<SessionDto>> Register([FromBody] RegistrationRequestDto request)
        {
            var result = await _userService.RegisterAsync(request);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("{username}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(string username)
        {
            var user = await _userService.GetUserAsync(username);
            return Ok(user);
        }
    }
}
