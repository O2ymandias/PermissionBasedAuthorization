using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermissionBasedAuthorization.Constants;
using PermissionBasedAuthorization.ViewModels;
using System.Security.Claims;
using UserManagementWithIdentity.ViewModels;

namespace UserManagementWithIdentity.Controllers
{
	[Authorize(Roles = nameof(Roles.SuperAdmin))]
	public class RolesController : Controller
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ILogger<RolesController> _logger;

		public RolesController(RoleManager<IdentityRole> roleManager,
			ILogger<RolesController> logger)
		{
			_roleManager = roleManager;
			_logger = logger;
		}
		public async Task<IActionResult> Index()
		{
			return View(await _roleManager.Roles.ToListAsync());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateRoleVM model)
		{
			if (ModelState.IsValid)
			{
				bool isRoleExists = await _roleManager.RoleExistsAsync(model.RoleName.Trim());

				if (!isRoleExists)
					await _roleManager.CreateAsync(new IdentityRole() { Name = model.RoleName.Trim() });
				else
					ModelState.AddModelError(nameof(CreateRoleVM.RoleName), "Role Already Exists!");
			}
			return View(nameof(Index), await _roleManager.Roles.ToListAsync());
		}

		[HttpGet]
		public async Task<IActionResult> ManagePermissions([FromRoute] string id)
		{
			var role = await _roleManager.FindByIdAsync(id);
			if (role is null)
				return NotFound();

			var roleClaims = (await _roleManager.GetClaimsAsync(role))
				.Select(roleClaim => roleClaim.Value)
				.ToList();

			var allPermissions = Permissions.GenerateAllPermissions();

			var modelPermissions = allPermissions.Select(permission => new CheckBoxVM()
			{
				DisplayValue = permission,
				IsSelected = roleClaims.Any(roleClaim => roleClaim == permission)
			}).ToList();

			var model = new RolePermissionsVM()
			{
				RoleId = role.Id,
				RoleName = role.Name ?? string.Empty,
				Permissions = modelPermissions
			};
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ManagePermissions([FromRoute] string id, RolePermissionsVM inputModel)
		{
			if (id != inputModel.RoleId)
				return BadRequest();

			var role = await _roleManager.FindByIdAsync(id);
			if (role is null)
				return NotFound();

			if (!ModelState.IsValid)
				return View(inputModel);

			var roleClaims = await _roleManager.GetClaimsAsync(role);

			foreach (var roleClaim in roleClaims.Where(r => r.Type == Permissions.Type))
			{
				var result = await _roleManager.RemoveClaimAsync(role, roleClaim);
				if (!result.Succeeded)
					foreach (var error in result.Errors)
					{
						_logger.LogError("Error: {Description}", error.Description);
						ModelState.AddModelError("", "Unexpected Error!");
						return View(inputModel);
					}
			}

			foreach (var selectedClaim in inputModel.Permissions.Where(p => p.IsSelected))
			{
				var result = await _roleManager.AddClaimAsync(role, new Claim(Permissions.Type, selectedClaim.DisplayValue));
				if (!result.Succeeded)
					foreach (var error in result.Errors)
					{
						_logger.LogError("Error: {Description}", error.Description);
						ModelState.AddModelError("", "Unexpected Error!");
						return View(inputModel);
					}
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
