using System.ComponentModel.DataAnnotations;

namespace UserManagementWithIdentity.ViewModels
{
	public class UserVM
	{
		public string UserId { get; set; }

		[Display(Name = "User Name")]
		public string UserName { get; set; }

		[EmailAddress]
		public string Email { get; set; }
		public IList<string> Roles { get; set; }
	}
}
