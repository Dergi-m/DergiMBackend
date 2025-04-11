using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		protected ResponceDto _responceDto;

		public UserController(IUserService userRepository)
		{
			_userService = userRepository;
			_responceDto = new();
		}

		[HttpGet("{organisationId:int?}")]
		public async Task<ResponceDto> Get(int? organisationId = null)
		{
			try
			{
				var users = await _userService.GetUsersAsync();
				if (users == null)
				{
					throw new Exception("Unexpected result - no users found");
				}
				_responceDto.Success = true;
				_responceDto.Result = users;
				_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responceDto.Success = false;
				_responceDto.Message = ex.Message;
			}
			return _responceDto;
		}

		[HttpGet("{username}")]
		public async Task<ResponceDto> Get(string username)
		{
			try
			{
				var user = await _userService.GetUserAsync(username);
				if (user == null)
				{
					throw new Exception("No user with this username");
				}
				_responceDto.Success = true;
				_responceDto.Result = user;
				_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responceDto.Success = false;
				_responceDto.Message = ex.Message;
			}
			return _responceDto;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
		{
			var tokendto = await _userService.Login(model);
			if (tokendto == null || string.IsNullOrEmpty(tokendto.AccessToken))
			{
				_responceDto.StatusCode = System.Net.HttpStatusCode.BadRequest;
				_responceDto.Success = false;
				_responceDto.Message = "Username or password is incorrect";
				return BadRequest(_responceDto);
			}
			_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			_responceDto.Success = true;
			_responceDto.Result = tokendto;
			return Ok(_responceDto);
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
		{
			bool isNameUnique = _userService.IsUniqueUser(model.UserName);
			if (!isNameUnique)
			{
				_responceDto.StatusCode = System.Net.HttpStatusCode.BadRequest;
				_responceDto.Success = false;
				_responceDto.Message = "Username already exists";
				return BadRequest(_responceDto);
			}
			var user = await _userService.Register(model);
			if (user == null)
			{
				_responceDto.StatusCode = System.Net.HttpStatusCode.BadRequest;
				_responceDto.Success = false;
				_responceDto.Message = "Error while registering";
				return BadRequest(_responceDto);
			}
			_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			_responceDto.Success = true;
			return Ok(_responceDto);
		}

		[Authorize(Roles = SD.RoleADMIN)]
		[HttpPost("assignToRole")]
		public async Task<IActionResult> AssignUserToRole([FromBody] RegistrationRequestDto model)
		{
			var user = await _userService.AssignUserToRole(model);
			if (user == null)
			{
				_responceDto.StatusCode = System.Net.HttpStatusCode.BadRequest;
				_responceDto.Success = false;
				_responceDto.Message = "Error while assigning role";
				return BadRequest(_responceDto);
			}
			_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			_responceDto.Success = true;
			return Ok(_responceDto);
		}
	}
}
