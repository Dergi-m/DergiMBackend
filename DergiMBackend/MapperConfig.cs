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
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                    .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age))
                    .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender));


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

                config.CreateMap<ProjectInvitation, ProjectInvitationDto>()
                    .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
                    .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Project.Id.ToString()))
                    .ForMember(dest => dest.SenderUserId, opt => opt.MapFrom(src => src.Project.CreatorId))
                    .ForMember(dest => dest.SenderUserName, opt => opt.MapFrom(src => src.Project.Creator.UserName))
                    .ForMember(dest => dest.TargetUserId, opt => opt.MapFrom(src => src.TargetUser.Id))
                    .ForMember(dest => dest.TargetUserName, opt => opt.MapFrom(src => src.TargetUser.UserName));

            });

            return mappingConfig;
        }
    }
}
