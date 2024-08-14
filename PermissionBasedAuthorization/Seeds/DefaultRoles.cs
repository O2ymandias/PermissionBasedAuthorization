using Microsoft.AspNetCore.Identity;
using PermissionBasedAuthorization.Constants;

namespace PermissionBasedAuthorization.Seeds
{
	public static class DefaultRoles
	{
		public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger<Program> logger)
		{
			if (!roleManager.Roles.Any())
			{
				try
				{
					var superAdminCreateResult = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.SuperAdmin)));
					LogCreationResult(superAdminCreateResult, nameof(Roles.SuperAdmin), logger);

					var adminCreateResult = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Admin)));
					LogCreationResult(adminCreateResult, nameof(Roles.Admin), logger);


					var basicCreateResult = await roleManager.CreateAsync(new IdentityRole(nameof(Roles.Basic)));
					LogCreationResult(basicCreateResult, nameof(Roles.Basic), logger);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error: {Message}", ex.Message);
					throw;
				}
			}
		}

		private static void LogCreationResult(IdentityResult creationResult, string roleName, ILogger<Program> logger)
		{
			if (creationResult.Succeeded)
				logger.LogInformation("Role '{RoleName}' is created successfully", roleName);
			else
				foreach (var error in creationResult.Errors)
					logger.LogError("Error: {Description} while creating '{RoleName}'", error.Description, roleName);
		}
	}
}
