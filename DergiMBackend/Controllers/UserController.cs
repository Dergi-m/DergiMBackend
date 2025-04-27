using DergiMBackend.Authorization;
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

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            try
            {
                var session = await _userService.LoginAsync(model);
                return Ok(session);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var isUnique = await _userService.IsUserUniqueAsync(model.UserName);
            if (!isUnique)
                return BadRequest(new { error = "Username already exists" });

            try
            {
                var session = await _userService.RegisterAsync(model);
                return Ok(session);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{username}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await _userService.GetUserAsync(username);
            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(user);
        }

        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }
    }
}
