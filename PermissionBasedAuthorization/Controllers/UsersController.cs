﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermissionBasedAuthorization.Constants;
using UserManagementWithIdentity.ViewModels;

namespace PermissionBasedAuthorization.Controllers
{
    [Authorize(Roles = nameof(Roles.SuperAdmin))]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserVM>(users.Count);

            foreach (var user in users)
                model.Add(new UserVM
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            return View(model);
        }

        private async Task<UserRolesVM> CreateUserRolesAsync(IdentityUser user, List<IdentityRole> roles)
        {
            var rolesVM = new List<CheckBoxVM>(roles.Count);

            foreach (var role in roles)
            {
                bool isSelected = await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty);
                rolesVM.Add(new CheckBoxVM()
                {
                    DisplayValue = role.Name ?? string.Empty,
                    IsSelected = isSelected
                });
            }

            return new UserRolesVM()
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Roles = rolesVM
            };

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles
                .Select(role => new CheckBoxVM() { DisplayValue = role.Name ?? string.Empty })
                .ToListAsync();

            return View(new CreateUserVM() { Roles = roles });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!model.Roles.Any(role => role.IsSelected))
            {
                ModelState.AddModelError(nameof(CreateUserVM.Roles), "At least 1 role must be assigned.");
                return View(model);
            }

            var uniqueEmail = await _userManager.FindByEmailAsync(model.Email);
            if (uniqueEmail is not null)
            {
                ModelState.AddModelError(nameof(CreateUserVM.Email), "Email Already Taken!!");
                return View(model);
            }

            var uniqueUserName = await _userManager.FindByNameAsync(model.Email);
            if (uniqueUserName is not null)
            {
                ModelState.AddModelError(nameof(CreateUserVM.UserName), "UserName Already Taken!!");
                return View(model);
            }

            var user = new IdentityUser
            {
                Email = model.Email,
                UserName = model.UserName
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Unexpected Error While Trying To Create This User");
                return View(model);
            }

            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(sr => sr.DisplayValue);
            var addToRolesResult = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!addToRolesResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "User Created, But There Is Unexpected Error While Trying To Assign This User To Specific Roles");
                return View(model);
            }

            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var model = await InitializeEditUserVM(user);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] string id, EditUserVM inputModel)
        {
            if (id != inputModel.UserId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(inputModel);

            var user = await _userManager.FindByIdAsync(inputModel.UserId);
            if (user is null)
                return NotFound();

            var userLocallyUpdated = await UpdateUserBasicInformation(user, inputModel);
            if (!userLocallyUpdated)
                return View(inputModel);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "An Error Occurred While Updating User");
                return View(inputModel);
            }

            var rolesUpdated = await UpdateUserRolesAsync(user, inputModel.Roles);
            if (!rolesUpdated)
                return View(inputModel);

            return RedirectToAction(nameof(Index));
        }


        private async Task<EditUserVM> InitializeEditUserVM(IdentityUser user)
        {
            var allRoles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            var userRoles = new List<CheckBoxVM>();
            foreach (var role in allRoles)
            {
                var isSelected = await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty);
                userRoles.Add(new CheckBoxVM
                {
                    DisplayValue = role.Name ?? string.Empty,
                    IsSelected = isSelected
                });
            }

            var model = new EditUserVM()
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Roles = userRoles
            };
            return model;
        }
        private async Task<bool> UpdateUserRolesAsync(IdentityUser user, List<CheckBoxVM> roles)
        {
            var selectedRoles = roles.Where(role => role.IsSelected).ToList();
            foreach (var selectedRole in selectedRoles)
            {
                if (!await _userManager.IsInRoleAsync(user, selectedRole.DisplayValue))
                {
                    var addResult = await _userManager.AddToRoleAsync(user, selectedRole.DisplayValue);
                    if (!addResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, $"Unexpected error while trying to add the user to the role {selectedRole.DisplayValue}.");
                        return false;
                    }
                }
            }

            var unSelectedRoles = roles.Where(role => !role.IsSelected).ToList();
            foreach (var unSelectedRole in unSelectedRoles)
            {
                if (await _userManager.IsInRoleAsync(user, unSelectedRole.DisplayValue))
                {
                    var removeResult = await _userManager.RemoveFromRoleAsync(user, unSelectedRole.DisplayValue);
                    if (!removeResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, $"Unexpected error while trying to remove the user from the role {unSelectedRole.DisplayValue}.");
                        return false;
                    }
                }
            }

            return true;
        }
        private async Task<bool> UpdateUserBasicInformation(IdentityUser user, EditUserVM inputModel)
        {
            if (!inputModel.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var isTakenEmail = await _userManager.FindByEmailAsync(inputModel.Email);
                if (isTakenEmail is not null)
                {
                    ModelState.AddModelError(nameof(EditUserVM.Email), "Email Already Taken");
                    return false;
                }
                user.Email = inputModel.Email;
            }

            if (!inputModel.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase))
            {
                var isTakenUserName = await _userManager.FindByNameAsync(inputModel.UserName);
                if (isTakenUserName is not null)
                {
                    ModelState.AddModelError(nameof(EditUserVM.UserName), "UserName Already Taken");
                    return false;
                }
                user.UserName = inputModel.UserName;
            }
            return true;
        }
    }
}
