using Microsoft.AspNetCore.Identity;
using PermissionBasedAuthorization.Constants;
using System.Security.Claims;

namespace PermissionBasedAuthorization.Seeds
{
	public class DefaultUsers
	{
		public static async Task SeedUsersAsync(UserManager<IdentityUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ILogger<Program> logger)
		{
			if (!userManager.Users.Any())
			{
				var superAdminUser = new IdentityUser()
				{
					UserName = "SuperAdmin",
					Email = "SuperAdmin@domain.com",
					EmailConfirmed = true
				};

				var basicUser = new IdentityUser()
				{
					UserName = "BasicUser",
					Email = "BasicUser@domain.com",
					EmailConfirmed = true
				};

				try
				{
					#region Seeding SuperAdmin User

					var superAdminCreationResult = await userManager.CreateAsync(superAdminUser, "P@ssw0rd");
					LogCreationResult(superAdminCreationResult, superAdminUser.UserName, logger);

					var assigningToAllRolesResult = await userManager.AddToRolesAsync(superAdminUser, Enum.GetNames(typeof(Roles)));
					LogAssigningToRolesResult(assigningToAllRolesResult, superAdminUser.Id, Enum.GetNames(typeof(Roles)), logger);

					await AddClaimsForSuperAdminUser(roleManager, logger);

					#endregion

					#region Seeding Basic User

					var basicUserCreationResult = await userManager.CreateAsync(basicUser, "P@ssw0rd");
					LogCreationResult(basicUserCreationResult, basicUser.UserName, logger);

					var assigningToBasicRoleResult = await userManager.AddToRoleAsync(basicUser, nameof(Roles.Basic));
					LogAssigningToRolesResult(assigningToBasicRoleResult, basicUser.Id, [nameof(Roles.Basic)], logger);

					#endregion
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error while seeding default users: {Message}", ex.Message);
					throw;
				}
			}
		}

		private static void LogCreationResult(IdentityResult creationResult, string userName, ILogger<Program> logger)
		{
			if (creationResult.Succeeded)
				logger.LogInformation("User '{UserName}' is created successfully", userName);

			else
				foreach (var error in creationResult.Errors)
					logger.LogError("Error: {Description} while creating user '{UserName}'", error.Description, userName);
		}

		private static void LogAssigningToRolesResult(IdentityResult assigningResult, string userId,
			string[] roles, ILogger<Program> logger)
		{
			if (assigningResult.Succeeded)
				logger.LogInformation("User #{UserId} is successfully assigned to role '{}'", userId, string.Join(", ", roles));

			else
				foreach (var error in assigningResult.Errors)
					logger.LogError("Error: {Description} while assigning user #{UserId}' to role '{}'",
						error.Description, userId, string.Join(", ", roles));
		}

		private static async Task AddClaimsForSuperAdminUser(RoleManager<IdentityRole> roleManager, ILogger<Program> logger)
		{
			var superAdminRole = await roleManager.FindByNameAsync(nameof(Roles.SuperAdmin));

			if (superAdminRole is not null)
			{
				var permissions = Permissions.GeneratePermissionsForModule(nameof(Modules.Products));
				var currentRoleClaims = await roleManager.GetClaimsAsync(superAdminRole);

				foreach (var permission in permissions)
				{
					if (!currentRoleClaims.Any(c => c.Type == Permissions.Type && c.Value == permission))
					{
						var addClaimResult = await roleManager.AddClaimAsync(superAdminRole, new Claim(Permissions.Type, permission));
						if (addClaimResult.Succeeded)
							logger.LogInformation("Permission '{Permission}' has been added to role '{Name}' Successfully", permission, superAdminRole.Name);
						else
							foreach (var error in addClaimResult.Errors)
								logger.LogError("Error occurred while adding permission '{Permission}' to role {Name}", permission, superAdminRole.Name);
					}
				}
			}
		}
	}
}
