using AutoMapper;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;

namespace DergiMBackend
{
    public static class MapperConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                // --- User mappings ---
                config.CreateMap<ApplicationUser, UserDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                    .ReverseMap();

                // --- Organisation mappings ---
                config.CreateMap<Organisation, OrganisationDto>()
                    .ReverseMap();

                // --- Organisation Role mappings ---
                config.CreateMap<OrganisationRole, OrganisationRoleDto>()
                    .ReverseMap();

                config.CreateMap<OrganisationRole, CreateOrganisationRoleDto>()
                    .ReverseMap();

                config.CreateMap<OrganisationRole, UpdateOrganisationRoleDto>()
                    .ReverseMap();

                // --- Membership mappings ---
                config.CreateMap<OrganisationMembership, CreateMembershipDto>()
                    .ReverseMap();

                config.CreateMap<OrganisationMembership, OrganisationMembershipDto>()
                    .ReverseMap();
            });

            return mappingConfig;
        }
    }
}
