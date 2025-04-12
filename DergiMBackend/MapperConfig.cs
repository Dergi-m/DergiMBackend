using AutoMapper;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;

namespace DergiMBackend
{
	public class MapperConfig
	{
		public static MapperConfiguration RegisterMaps()
		{
			var mapping = new MapperConfiguration(config =>
			{
				config.CreateMap<Project, ProjectDto>().ReverseMap();
				config.CreateMap<Organisation, OrganisationDto>().ReverseMap();
			});
			return mapping;
		}
	}
}
