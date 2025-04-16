using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DergiMBackend.Services.IServices;

namespace DergiMBackend.Controllers
{
	[Route("api/organisations")]
	[Authorize]
	[ApiController]
	public class OrganisationController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private ResponceDto _responceDto;
		public ITokenService _tokenService;

		public OrganisationController(ApplicationDbContext db, IMapper mapper, ITokenService tokenService)
		{
			_db = db;
			_mapper = mapper;
			_responceDto = new ResponceDto();
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

		[HttpGet]
		public async Task<ResponceDto> Get()
		{
			try
			{
				ValidateRegisteredUser();
				List<Organisation> organisations = await _db.Organisation.ToListAsync();

				_responceDto.Success = true;
				_responceDto.Result = _mapper.Map<List<OrganisationDto>>(organisations);
				_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responceDto.Success = false;
				_responceDto.Message = ex.Message;
			}
			return _responceDto;
		}

		[HttpGet("{id:int}")]
		public async Task<ResponceDto> Get(int id)
		{
			try
			{
				ValidateRegisteredUser();
				var organisation = await _db.Organisation.FirstOrDefaultAsync(u => u.Id == id);

				_responceDto.Success = true;
				_responceDto.Result = _mapper.Map<OrganisationDto>(organisation);
				_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responceDto.Success = false;
				_responceDto.Message = ex.Message;
			}
			return _responceDto;
		}

		[HttpPost]
		public async Task<ResponceDto> Create(OrganisationDto organisationDto)
		{
			try
			{
				ValidateAdminRole();
				var organisation = _mapper.Map<Organisation>(organisationDto);

				await _db.Organisation.AddAsync(organisation);
				await _db.SaveChangesAsync();

				_responceDto.Success = true;
				_responceDto.Result = _mapper.Map<OrganisationDto>(organisation);
				_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responceDto.Success = false;
				_responceDto.Message = ex.Message;
			}
			return _responceDto;
		}

		[HttpPut]
		public async Task<ResponceDto> Update(OrganisationDto organisationDto)
		{
			try
			{
				ValidateAdminRole();
				var organisation = await _db.Organisation.FirstOrDefaultAsync(u => u.Id == organisationDto.Id);

				organisation.Description = organisationDto.Description;
				organisation.Name = organisationDto.Name;

				_db.Organisation.Update(organisation);
				await _db.SaveChangesAsync();

				_responceDto.Success = true;
				_responceDto.Result = _mapper.Map<OrganisationDto>(organisation);
				_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responceDto.Success = false;
				_responceDto.Message = ex.Message;
			}
			return _responceDto;
		}

		[Authorize(Roles = SD.RoleADMIN)]
		[HttpDelete("{id:int}")]
		public async Task<ResponceDto> Delete(int id)
		{
			try
			{
				ValidateAdminRole();
				var organisation = await _db.Organisation.FirstOrDefaultAsync(u => u.Id == id);

				_db.Organisation.Remove(organisation);
				await _db.SaveChangesAsync();

				_responceDto.Success = true;
				_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responceDto.Success = false;
				_responceDto.Message = ex.Message;
			}
			return _responceDto;
		}
	}
}
