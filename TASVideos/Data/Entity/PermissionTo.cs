using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TASVideos.Attributes;

namespace TASVideos.Data.Entity
{
	/// <summary>
	/// Represents the most granular level permissions possible in the site. All site code is based on these permissions
	/// The <see cref="Role" /> table represents a group of permissions that can be assigned to a <seealso cref="User"/>
	/// </summary>
	public enum PermissionTo
	{
		[Display(Name = "Edit Wiki Pages")]
		[Group("Editing")]
		[Description("The ability to edit basic wiki pages. This is the most basic editor privilege but some pages may be restrited to other privileges.")]
		EditWikiPages = 1,

		[Display(Name = "Edit Game Resources")]
		[Group("Editing")]
		[Description("The ability to edit Game Resource wiki pages. These are basic game information and are considered separate from general wiki pages.")]
		EditGameResources = 2,

		[Display(Name = "Edit System Pages")]
		[Group("Editing")]
		[Description("The ability to edit System Wiki pages. These pages are more fundamental to the site behavior whan basic wiki pages.")]
		EditSystemPages = 3,

		[Display(Name = "Edit Roles")]
		[Group("Editing")]
		[Description("The ability to create and edit roles and add/remove privileges to those roles.")]
		EditRoles = 4,

		[Display(Name = "Delete Roles")]
		[Group("UserAdministration")]
		[Description("The ability to delete an existing role.")]
		DeleteRoles = 5,

		[Display(Name = "View Users")]
		[Group("UserAdministration")]
		[Description("The ability to see other user's profile data in read-only form.")]
		ViewUsers = 6,

		[Display(Name = "Edit Users")]
		[Group("UserAdministration")]
		[Description("The ability to edit basic information about another user.")]
		EditUsers = 7,

		[Display(Name = "Edit Users UserName")]
		[Group("UserAdministration")]
		[Description("The ability to change another user's UserName. Users with this permission should also have the EditUsers permission.")]
		EditUsersUserName = 8,

		[Display(Name = "Assign Roles")]
		[Group("UserAdministration")]
		[Description("The ability to assign Roles to any User with some restrictions. A role can only be assigned if all permissions within it are marked as assignable by a role the user has.")]
		AssignRoles = 9
	}
}
