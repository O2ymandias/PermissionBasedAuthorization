using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PermissionBasedAuthorization.Data;
using PermissionBasedAuthorization.Extensions;
using PermissionBasedAuthorization.Policies;

namespace PermissionBasedAuthorization
{
	public class Program
	{
		private static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);


			#region Add services to the container.

			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			builder.Services.AddIdentity<IdentityUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultUI();

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddControllersWithViews();

			builder.Services.AddSingleton(typeof(IAuthorizationHandler), typeof(PermissionHandler));
			builder.Services.AddSingleton(typeof(IAuthorizationPolicyProvider), typeof(PermissionPolicyProvider));
			builder.Services.Configure<SecurityStampValidatorOptions>(config =>
			{
				config.ValidationInterval = TimeSpan.Zero;
			});

			#endregion

			var app = builder.Build();

			await app.InitializeDataAsync();


			#region Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			app.MapRazorPages();

			app.Run();

			#endregion
		}
	}
}