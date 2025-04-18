using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
	[Route("api/users")]
	[Authorize]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly ISessionService _tokenService;
		protected ResponseDto _responceDto;

		public UserController(IUserService userRepository, ISessionService tokenService)
		{
			_userService = userRepository;
			_responceDto = new();
			_tokenService = tokenService;
		}

		private void ValidateRegisteredUser()
		{
			var sessionToken = Request.Headers["SessionToken"].ToString();
			if (string.IsNullOrEmpty(sessionToken))
			{
				throw new UnauthorizedAccessException("SessionToken is required.");
			}

			_tokenService.ValidateSessionToken(sessionToken);
		}

		private void ValidateAdminRole()
		{
			var sessionToken = Request.Headers["SessionToken"].ToString();
			if (string.IsNullOrEmpty(sessionToken))
			{
				throw new UnauthorizedAccessException("SessionToken is required.");
			}

			var role = _tokenService.ValidateSessionToken(sessionToken);
			if (role != SD.RoleADMIN)
			{
				throw new UnauthorizedAccessException("You do not have the required ADMIN role.");
			}
		}

		[HttpGet("{organisationId:int?}")]
		public async Task<ResponseDto> GetOrganizationUsers(string? organizationUniqueName = null)
		{
			try
			{
				ValidateRegisteredUser();
				var users = await _userService.GetUsersAsync(organizationUniqueName);

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
		public async Task<ResponseDto> Get(string username)
		{
			try
			{
				ValidateRegisteredUser();
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
			if (tokendto == null || string.IsNullOrEmpty(tokendto.SessionToken))
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
			ValidateAdminRole();
			var user = await _userService.AssignUserToRole(model.UserName, model.Role.Id.ToString());
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

		[Authorize(Roles = SD.RoleADMIN)]
		[HttpPost("assignToOrganisation")]
		public async Task<IActionResult> AssignUserToOrganisation([FromBody] ApplicationUser user)
		{
			ValidateAdminRole();
			var result = await _userService.AssignUserToOrganisation(user);
			if (!result)
			{
				_responceDto.StatusCode = System.Net.HttpStatusCode.BadRequest;
				_responceDto.Success = false;
				_responceDto.Message = "Error while assigning user to organisation";
				return BadRequest(_responceDto);
			}
			_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			_responceDto.Success = true;
			return Ok(_responceDto);
		}
	}
}
