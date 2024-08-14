using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PermissionBasedAuthorization.Constants;

namespace PermissionBasedAuthorization.Policies
{
	public class PermissionPolicyProvider : IAuthorizationPolicyProvider
	{
		public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }
		public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
		{
			FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
		}
		public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
			FallbackPolicyProvider.GetDefaultPolicyAsync();
		public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
			FallbackPolicyProvider.GetFallbackPolicyAsync();

		public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
		{
			if (policyName.StartsWith(Permissions.Type, StringComparison.OrdinalIgnoreCase))
			{
				var policyBuilder = new AuthorizationPolicyBuilder();
				policyBuilder.AddRequirements(new PermissionRequirement(policyName));
				var policy = policyBuilder.Build();
				return Task.FromResult<AuthorizationPolicy?>(policy);
			}
			return FallbackPolicyProvider.GetPolicyAsync(policyName);
		}
	}
}
