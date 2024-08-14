using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PermissionBasedAuthorization.Controllers.Api
{
	[Route("api/[controller]")]
	[ApiController]
	public class RolesController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> _roleManager;

		public RolesController(RoleManager<IdentityRole> roleManager)
		{
			this._roleManager = roleManager;
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete([FromRoute] string id)
		{
			var role = await _roleManager.FindByIdAsync(id);
			if (role is null)
				return NotFound();

			var result = await _roleManager.DeleteAsync(role);

			return result.Succeeded
				? Ok(result)
				: StatusCode(StatusCodes.Status500InternalServerError);
		}
	}
}
