using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PermissionBasedAuthorization.Constants;

namespace PermissionBasedAuthorization.Controllers
{
	public class ProductsController : Controller
	{
		[Authorize(Policy = Permissions.Products.View)]
		public IActionResult Index()
		{
			return View();
		}

		[Authorize(Policy = Permissions.Products.Create)]
		public IActionResult Create()
		{
			return View();
		}

		[Authorize(Policy = Permissions.Products.Edit)]
		public IActionResult Edit()
		{
			return View();
		}

		[Authorize(Policy = Permissions.Products.Delete)]
		public IActionResult Delete()
		{
			return View();
		}
	}
}
