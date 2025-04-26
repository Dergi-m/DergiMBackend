using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace DergiMBackend.Controllers
{
    [Route("api/organisations")]
    [Authorize]
    [ApiController]
    public class OrganisationController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ISessionService _sessionService;
        private readonly ResponseDto _responseDto = new();

        public OrganisationController(ApplicationDbContext db, IMapper mapper, ISessionService sessionService)
        {
            _db = db;
            _mapper = mapper;
            _sessionService = sessionService;
        }

        private void ValidateSession(bool requireAdmin = false)
        {
            var sessionToken = Request.Headers["SessionToken"].ToString();
            if (string.IsNullOrEmpty(sessionToken))
                throw new UnauthorizedAccessException("SessionToken is required.");

            var role = _sessionService.ValidateSessionToken(sessionToken);
            if (requireAdmin && role != SD.RoleADMIN)
                throw new UnauthorizedAccessException("Admin privileges required.");
        }

        [HttpGet]
        public async Task<ResponseDto> GetAllOrganisations()
        {
            try
            {
                ValidateSession();
                var organisations = await _db.Organisations.ToListAsync();
                _responseDto.Result = _mapper.Map<List<OrganisationDto>>(organisations);
                _responseDto.Success = true;
                _responseDto.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _responseDto.Success = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpGet("{uniqueName}")]
        public async Task<ResponseDto> GetOrganisation(string uniqueName)
        {
            try
            {
                ValidateSession();
                var organisation = await _db.Organisations.FirstOrDefaultAsync(x => x.UniqueName == uniqueName);

                if (organisation == null)
                    throw new KeyNotFoundException("Organisation not found.");

                _responseDto.Result = _mapper.Map<OrganisationDto>(organisation);
                _responseDto.Success = true;
                _responseDto.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _responseDto.Success = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpPost]
        public async Task<ResponseDto> CreateOrganisation([FromBody] OrganisationDto organisationDto)
        {
            try
            {
                ValidateSession(requireAdmin: true);

                var organisation = _mapper.Map<Organisation>(organisationDto);
                await _db.Organisations.AddAsync(organisation);
                await _db.SaveChangesAsync();

                _responseDto.Result = _mapper.Map<OrganisationDto>(organisation);
                _responseDto.Success = true;
                _responseDto.StatusCode = HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                _responseDto.Success = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpPut]
        public async Task<ResponseDto> UpdateOrganisation([FromBody] OrganisationDto organisationDto)
        {
            try
            {
                ValidateSession(requireAdmin: true);

                var organisation = await _db.Organisations.FirstOrDefaultAsync(x => x.UniqueName == organisationDto.UniqueName);
                if (organisation == null)
                    throw new KeyNotFoundException("Organisation not found.");

                organisation.Name = organisationDto.Name;
                organisation.Description = organisationDto.Description;

                _db.Organisations.Update(organisation);
                await _db.SaveChangesAsync();

                _responseDto.Result = _mapper.Map<OrganisationDto>(organisation);
                _responseDto.Success = true;
                _responseDto.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _responseDto.Success = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpDelete("{uniqueName}")]
        public async Task<ResponseDto> DeleteOrganisation(string uniqueName)
        {
            try
            {
                ValidateSession(requireAdmin: true);

                var organisation = await _db.Organisations.FirstOrDefaultAsync(x => x.UniqueName == uniqueName);
                if (organisation == null)
                    throw new KeyNotFoundException("Organisation not found.");

                _db.Organisations.Remove(organisation);
                await _db.SaveChangesAsync();

                _responseDto.Success = true;
                _responseDto.StatusCode = HttpStatusCode.NoContent;
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