using Microsoft.AspNetCore.Identity;
using PermissionBasedAuthorization.Seeds;

namespace PermissionBasedAuthorization.Extensions
{
	public static class InitialData
	{
		public static async Task InitializeDataAsync(this WebApplication app)
		{
			using var scope = app.Services.CreateScope();
			var scopedServices = scope.ServiceProvider;

			var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();
			var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
			var logger = scopedServices.GetRequiredService<ILogger<Program>>();

			/*
			ILogger<T> is indeed a singleton by default in ASP.NET Core.
			The logging infrastructure is designed to be used as a singleton,
			and services such as ILogger<T> are typically registered as singletons in the dependency injection container.
			Given that ILogger<T> is a singleton, you can safely resolve it from the root service provider,
			and there’s no issue with using app.Services.GetRequiredService<ILogger<Program>>() directly.
			However, for consistency and clarity, it is often recommended to resolve all services in the same way,
			especially if you are working within a scope.
			 */

			try
			{
				await DefaultRoles.SeedRolesAsync(roleManager, logger);
				await DefaultUsers.SeedUsersAsync(userManager, roleManager, logger);
				logger.LogInformation("Data has been seeded.");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred while seeding initial data : {Message}", ex.Message);
			}
		}
	}
}
