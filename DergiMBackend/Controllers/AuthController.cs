using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
	[Route("auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly ITokenService _tokenService;
		private ResponceDto _responceDto;

		public AuthController(ITokenService tokenService)
		{
			_tokenService = tokenService;
		}

		[HttpGet]
		public IActionResult GenerateToken([FromHeader] string clientId, string clientsecret)
		{
			try
			{
				var token = _tokenService.GenerateToken(clientId, clientsecret);
				return Ok(new { AccessToken = token });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}
	}
}
