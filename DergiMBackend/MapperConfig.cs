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
                    .ForMember(dest => dest.Projects, opt => opt.MapFrom(src => src.Projects))
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

                // --- Project mapping (summary only) ---
                config.CreateMap<Project, ProjectSummaryDto>();
                config.CreateMap<Project, ProjectDto>()
                       .ForMember(dest => dest.MemberIds, opt => opt.MapFrom(src => src.Members.Select(m => m.Id).ToList()));
                config.CreateMap<Project, ProjectDetailsDto>()
                    .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members))
                    .ForMember(dest => dest.Invitations, opt => opt.MapFrom(src => src.Invitations));

                // --- Organisation mappings ---
                config.CreateMap<Organisation, OrganisationDto>().ReverseMap();

                // --- Organisation Role mappings ---
                config.CreateMap<OrganisationRole, OrganisationRoleDto>().ReverseMap();
                config.CreateMap<OrganisationRole, CreateOrganisationRoleDto>().ReverseMap();
                config.CreateMap<OrganisationRole, UpdateOrganisationRoleDto>().ReverseMap();

                // --- Membership mappings ---
                config.CreateMap<OrganisationMembership, CreateMembershipDto>().ReverseMap();
                config.CreateMap<OrganisationMembership, OrganisationMembershipDto>().ReverseMap();

                config.CreateMap<ProjectInvitation, ProjectInvitationDto>()
                    .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId)) // <- this line is the fix
                    .ForMember(dest => dest.SenderUserId, opt => opt.MapFrom(src => src.SenderUserId))
                    .ForMember(dest => dest.TargetUserId, opt => opt.MapFrom(src => src.TargetUserId));

                // --- Project File mappings ---
                config.CreateMap<ProjectFile, ProjectFileDto>().ReverseMap();
                config.CreateMap<ProjectFile, CreateProjectFileDto>().ReverseMap();


            });

            return mappingConfig;
        }
    }

}
