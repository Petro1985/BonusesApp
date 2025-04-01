using BonusesApp.Core.Services.Account;
using Microsoft.AspNetCore.Authorization;

namespace BonusesApp.Server.Authorization.Requirements;

public class ViewRoleAuthorizationRequirement : IAuthorizationRequirement
{
}

public class ViewRoleAuthorizationHandler : AuthorizationHandler<ViewRoleAuthorizationRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ViewRoleAuthorizationRequirement requirement, string roleName)
    {
        if (context.User == null)
            return Task.CompletedTask;

        if (context.User.HasClaim(CustomClaims.Permission, ApplicationPermissions.ViewRoles)
            || context.User.IsInRole(roleName))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}