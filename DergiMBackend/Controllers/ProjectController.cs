using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DergiMBackend.Controllers
{
	[Route("api/projects")]
	[Authorize]
	[ApiController]
	public class ProjectController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IConfiguration _configuration;
		private readonly ISessionService _tokenService;
		private ResponseDto _responceDto;

		public ProjectController(ApplicationDbContext db, IMapper mapper, IConfiguration configuration, ISessionService tokenService)
		{
			_db = db;
			_mapper = mapper;
			_responceDto = new ResponseDto();
			_configuration = configuration;
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
		public async Task<ResponseDto> Get(int? organisationId = null)
		{
			try
			{
				ValidateRegisteredUser();
				List<Project> projects = await _db.Projects.Include("ProjectFiles").ToListAsync();
				
				if (organisationId != null)
				{
					projects = projects.Where(u=> u.OrganisationId == organisationId).ToList();
				}

				_responceDto.Success = true;
				_responceDto.Result = _mapper.Map<List<ProjectDto>>(projects);
				_responceDto.StatusCode = System.Net.HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_responceDto.Success = false;
				_responceDto.Message = ex.Message;
			}
			return _responceDto;
		}

		[HttpGet("getProject/{projectId:int}")]
		public async Task<ResponseDto> Get(int projectId)
		{
			try
			{
				ValidateRegisteredUser();
				var project  = await _db.Projects.Include("ProjectFiles").FirstOrDefaultAsync(u => u.Id == projectId);

				_responceDto.Success = true;
				_responceDto.Result = _mapper.Map<ProjectDto>(project);
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
		public async Task<ResponseDto> Create(ProjectDto projectDto)
		{
			try
			{
				ValidateRegisteredUser();
				var project = _mapper.Map<Project>(projectDto);

				await _db.Projects.AddAsync(project);
				await _db.SaveChangesAsync();

				_responceDto.Success = true;
				_responceDto.Result = _mapper.Map<ProjectDto>(project);
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
		public async Task<ResponseDto> Update(ProjectDto projectDto)
		{
			try
			{
				ValidateAdminRole();
				var project = await _db.Projects.FirstOrDefaultAsync(u => u.Id == projectDto.Id);

				project.Description = projectDto.Description;
				project.Name = projectDto.Name;

				_db.Projects.Update(project);
				await _db.SaveChangesAsync();

				_responceDto.Success = true;
				_responceDto.Result = _mapper.Map<ProjectDto>(project);
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
		public async Task<ResponseDto> Delete(int id)
		{
			try
			{
				ValidateAdminRole();
				var project = await _db.Projects.FirstOrDefaultAsync(u => u.Id == id);

				_db.Projects.Remove(project);
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

		[HttpPost("addFile")]
		public async Task<ResponseDto> AddFile([FromForm]ProjectFileDto projectFileDto)
		{
			try
			{
				ValidateRegisteredUser();
				var extension = Path.GetExtension(projectFileDto.File.FileName);
				if (!SD.EXTENSIONS.Any(x => x == extension))
				{
					throw new Exception("Unaccepted file format");
				}
				string guid = Guid.NewGuid().ToString();
				string fileName = guid + Path.GetExtension(projectFileDto.File.FileName);
				string filePath = @"wwwroot\files\" + projectFileDto.ProjectId + @"\" + fileName;

				var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

				var directoryPath = Path.GetDirectoryName(filePathDirectory);

				var rootPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files"));
				var resolvedPath = Path.GetFullPath(filePathDirectory);

				if (!resolvedPath.StartsWith(rootPath))
				{
					throw new UnauthorizedAccessException("Invalid file path.");
				}

				if (!Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}

				using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
				{
					projectFileDto.File.CopyTo(fileStream);

				}
				var baseUrl = _configuration["ApiSettings:BaseUrl"];
				ProjectFile projectFile = new()
				{
					FileUrl = $"{baseUrl}/files/{projectFileDto.ProjectId}/{fileName}",
					LocalFileUrl = filePath,
					ProjectId = projectFileDto.ProjectId
				};

				await _db.ProjectFiles.AddAsync(projectFile);
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

		[HttpDelete("deleteFile/{id:int}")]
		public async Task<ResponseDto> DeleteFile(int id)
		{
			try
			{
				ValidateAdminRole();
				var FileToDelete = await _db.ProjectFiles.FirstOrDefaultAsync(u => u.Id == id);

				if (System.IO.File.Exists(FileToDelete?.LocalFileUrl))
				{
					System.IO.File.Delete(FileToDelete.LocalFileUrl);
				}

				_db.ProjectFiles.Remove(FileToDelete);
				await _db.SaveChangesAsync();

				_responceDto.Success = true;
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
