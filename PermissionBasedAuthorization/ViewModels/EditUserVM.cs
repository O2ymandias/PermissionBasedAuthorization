using System.ComponentModel.DataAnnotations;

namespace UserManagementWithIdentity.ViewModels
{
	public class EditUserVM
	{
		public string UserId { get; set; }

		[Required]
		[Display(Name = "User Name")]
		public string UserName { get; set; }


		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		public List<CheckBoxVM> Roles { get; set; }
	}
}
