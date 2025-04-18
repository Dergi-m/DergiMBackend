using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DergiMBackend.Controllers
{
	[Route("api/organisations")]
	[Authorize]
	[ApiController]
	public class OrganisationController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private ResponseDto _responseDto;
		public ISessionService _sessionService;

		public OrganisationController(ApplicationDbContext db, IMapper mapper, ISessionService tokenService)
		{
			_db = db;
			_mapper = mapper;
			_responseDto = new ResponseDto();
			_sessionService = tokenService;
		}

		private void ValidateRegisteredUser()
		{
			var sessionToken = Request.Headers["SessionToken"].ToString();
			if (string.IsNullOrEmpty(sessionToken))
			{
				throw new UnauthorizedAccessException("SessionToken is required.");
			}

			_sessionService.ValidateSessionToken(sessionToken);
		}

		private void ValidateAdminRole()
		{
			var sessionToken = Request.Headers["SessionToken"].ToString();
			if (string.IsNullOrEmpty(sessionToken))
			{
				throw new UnauthorizedAccessException("SessionToken is required.");
			}

			var role = _sessionService.ValidateSessionToken(sessionToken);
			if (role != SD.RoleADMIN)
			{
				throw new UnauthorizedAccessException("You do not have the required ADMIN role.");
			}
		}

		[HttpGet]
		public async Task<ResponseDto> Get()
		{
			try
			{
				ValidateRegisteredUser();
				List<Organisation> organisations = await _db.Organisation.ToListAsync();

				_responseDto.Success = true;
				_responseDto.Result = _mapper.Map<List<OrganisationDto>>(organisations);
				_responseDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responseDto.Success = false;
				_responseDto.Message = ex.Message;
			}
			return _responseDto;
		}

		[HttpGet("{id:int}")]
		public async Task<ResponseDto> Get(string uniqueName)
		{
			try
			{
				ValidateRegisteredUser();
				var organisation = await _db.Organisation.FirstOrDefaultAsync(org => org.UniqueName == uniqueName);

				_responseDto.Success = true;
				_responseDto.Result = _mapper.Map<OrganisationDto>(organisation);
				_responseDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responseDto.Success = false;
				_responseDto.Message = ex.Message;
			}
			return _responseDto;
		}

		[HttpPost]
		public async Task<ResponseDto> Create(OrganisationDto organisationDto)
		{
			try
			{
				ValidateAdminRole();
				var organisation = _mapper.Map<Organisation>(organisationDto);

				await _db.Organisation.AddAsync(organisation);
				await _db.SaveChangesAsync();

				_responseDto.Success = true;
				_responseDto.Result = _mapper.Map<OrganisationDto>(organisation);
				_responseDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responseDto.Success = false;
				_responseDto.Message = ex.Message;
			}
			return _responseDto;
		}

		[HttpPut]
		public async Task<ResponseDto> Update(OrganisationDto organisationDto)
		{
                try
			{
				ValidateAdminRole();
                var organisation = await _db.Organisation.FirstOrDefaultAsync(org => org.UniqueName == organisationDto.UniqueName);

				if(organisation == null)
				{
					throw new Exception("Organisation not found");
				}


                organisation.Description = organisationDto.Description!;

				_db.Organisation.Update(organisation);
				await _db.SaveChangesAsync();

				_responseDto.Success = true;
				_responseDto.Result = _mapper.Map<OrganisationDto>(organisation);
				_responseDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responseDto.Success = false;
				_responseDto.Message = ex.Message;
			}
			return _responseDto;
		}

		[Authorize(Roles = SD.RoleADMIN)]
		[HttpDelete("{id:int}")]
		public async Task<ResponseDto> Delete(string uniqueName)
		{
			try
			{
				ValidateAdminRole();
				var organisation = await _db.Organisation.FirstOrDefaultAsync(u => u.UniqueName == uniqueName);

				if(organisation != null)
				{
					_db.Organisation.Remove(organisation);
					await _db.SaveChangesAsync();

				}

				_responseDto.Success = true;
				_responseDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responseDto.Success = false;
				_responseDto.Message = ex.Message;
			}
			return _responseDto;
		}
	}
}
