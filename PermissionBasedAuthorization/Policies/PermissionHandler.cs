using Microsoft.AspNetCore.Authorization;
using PermissionBasedAuthorization.Constants;

namespace PermissionBasedAuthorization.Policies
{
	public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
		{
			if (context.User is not null && context.User.Claims.Any(c => c.Type == Permissions.Type && c.Value == requirement.Permission))
				context.Succeed(requirement);

			return Task.CompletedTask;
		}
	}
}
