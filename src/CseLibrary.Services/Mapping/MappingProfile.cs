using AutoMapper;
using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;

namespace CseLibrary.Services.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(d => d.Roles, o => o.Ignore());

        CreateMap<ApplicationRole, RoleDto>()
            .ForMember(d => d.PermissionIds,
                o => o.MapFrom(s => s.RolePermissions.Select(rp => rp.PermissionId)));

        CreateMap<Permission, PermissionDto>().ReverseMap();
        CreateMap<CreatePermissionDto, Permission>();
        CreateMap<UpdatePermissionDto, Permission>();

        CreateMap<GeneralConfig, GeneralConfigDto>().ReverseMap();
        CreateMap<CreateGeneralConfigDto, GeneralConfig>();
        CreateMap<UpdateGeneralConfigDto, GeneralConfig>();
    }
}
