using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Controllers
{
	[Route("api/projects")]
	[ApiController]
	public class ProjectController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private ResponceDto _responceDto;

		public ProjectController(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
			_responceDto = new ResponceDto();
		}

		[HttpGet("{organisationId:int?}")]
		public async Task<ResponceDto> Get(int? organisationId = null)
		{
			try
			{
				List<Project> projects = await _db.Projects.ToListAsync();
				
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
		public async Task<ResponceDto> Get(int projectId)
		{
			try
			{
				var project  = await _db.Projects.FirstOrDefaultAsync(u => u.Id == projectId);

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
		public async Task<ResponceDto> Create(ProjectDto projectDto)
		{
			try
			{
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
		public async Task<ResponceDto> Update(ProjectDto projectDto)
		{
			try
			{
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

		[HttpDelete("{id:int}")]
		public async Task<ResponceDto> Delete(int id)
		{
			try
			{
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
	}
}
