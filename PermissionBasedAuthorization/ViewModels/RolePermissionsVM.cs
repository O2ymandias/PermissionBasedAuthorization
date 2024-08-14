using UserManagementWithIdentity.ViewModels;

namespace PermissionBasedAuthorization.ViewModels
{
	public class RolePermissionsVM
	{
		public string RoleId { get; set; }
		public string RoleName { get; set; }
		public List<CheckBoxVM> Permissions { get; set; }
	}
}
