using Microsoft.AspNetCore.Authorization;

namespace PermissionBasedAuthorization.Policies
{
	public class PermissionRequirement : IAuthorizationRequirement
	{
		public string Permission { get; private set; }

		public PermissionRequirement(string permission)
		{
			Permission = permission;
		}
	}
}
