using AutoMapper;
using BonusesApp.Core.Models.Account;
using BonusesApp.Core.Models.Bonuses;
using BonusesApp.Core.Services.Account;
using BonusesApp.Server.ViewModels.Account;
using BonusesApp.Server.ViewModels.Bonuses;
using Microsoft.AspNetCore.Identity;

namespace BonusesApp.Server.Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserVM>()
            .ForMember(d => d.Roles, map => map.Ignore());
        CreateMap<UserVM, ApplicationUser>()
            .ForMember(d => d.Roles, map => map.Ignore())
            .ForMember(d => d.Id, map => map.Condition(src => src.Id != null));

        CreateMap<ApplicationUser, UserEditVM>()
            .ForMember(d => d.Roles, map => map.Ignore());
        CreateMap<UserEditVM, ApplicationUser>()
            .ForMember(d => d.Roles, map => map.Ignore())
            .ForMember(d => d.Id, map => map.Condition(src => src.Id != null));

        CreateMap<ApplicationUser, UserPatchVM>()
            .ReverseMap();

        CreateMap<ApplicationRole, RoleVM>()
            .ForMember(d => d.Permissions, map => map.MapFrom(s => s.Claims))
            .ForMember(d => d.UsersCount, map => map.MapFrom(s => s.Users != null ? s.Users.Count : 0))
            .ReverseMap();
        CreateMap<RoleVM, ApplicationRole>()
            .ForMember(d => d.Id, map => map.Condition(src => src.Id != null));

        CreateMap<IdentityRoleClaim<string>, ClaimVM>()
            .ForMember(d => d.Type, map => map.MapFrom(s => s.ClaimType))
            .ForMember(d => d.Value, map => map.MapFrom(s => s.ClaimValue))
            .ReverseMap();

        CreateMap<ApplicationPermission, PermissionVM>()
            .ReverseMap();

        CreateMap<IdentityRoleClaim<string>, PermissionVM>()
            .ConvertUsing(s => ((PermissionVM)ApplicationPermissions.GetPermissionByValue(s.ClaimValue))!);


        CreateMap<BonusesEntity, BonusesVM>()
            .ForMember(x => x.PhoneNumber, map => map.MapFrom(s => s.PhoneNumber))
            .ForMember(x => x.TotalCounter, map => map.MapFrom(s => s.TotalCount))
            .ForMember(x => x.CurrentCounter, map => map.MapFrom(s => s.CurrentCount))
            .ForMember(x => x.Setting, map => map.MapFrom(s => s.Setting))
            .ForMember(x => x.LastUpdate, map => map.MapFrom(s => s.UpdatedDate))
            .ReverseMap();

        CreateMap<AddBonusesRequest, BonusesEntity>()
            .ForMember(x => x.PhoneNumber, map => map.MapFrom(s => s.PhoneNumber))
            .ForMember(x => x.Setting, map => map.MapFrom(s => s.Setting))
            .ForMember(x => x.CurrentCount, map => map.MapFrom(s => s.CurrentCounter))
            .ForMember(x => x.TotalCount, map => map.MapFrom(s => s.TotalCounter))
            .ForMember(x => x.Id, map => map.Ignore())
            .ForMember(x => x.Name, map => map.MapFrom(x => x.Name));

        CreateMap<BonusesUpdateRequest, BonusesEntity>()
            .ForMember(x => x.PhoneNumber, map => map.MapFrom(s => s.PhoneNumber))
            .ForMember(x => x.Setting, map => map.MapFrom(s => s.Setting))
            .ForMember(x => x.CurrentCount, map => map.MapFrom(s => s.CurrentCounter))
            .ForMember(x => x.TotalCount, map => map.MapFrom(s => s.TotalCounter))
            .ForMember(x => x.Id, map => map.MapFrom(x => x.Id))
            .ForMember(x => x.Name, map => map.MapFrom(x => x.Name));
    }
}