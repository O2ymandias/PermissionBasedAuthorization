using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PermissionBasedAuthorization.Constants;

namespace UserManagementWithIdentity.Controllers.Api
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = nameof(Roles.SuperAdmin))]
	public class UsersController : ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;

		public UsersController(UserManager<IdentityUser> userManager)
		{
			_userManager = userManager;
		}

		[HttpDelete("{userId}")]
		public async Task<IActionResult> DeleteUser(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user is null)
				return NotFound();

			var deleteResult = await _userManager.DeleteAsync(user);
			return deleteResult.Succeeded
				? Ok()
				: StatusCode(StatusCodes.Status500InternalServerError);
		}
	}
}
