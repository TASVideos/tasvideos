using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a <seealso cref="TASVideos.Data.Entity.User"/> entry list of users
	/// </summary>
	public class UserListViewModel
    {
		public int Id { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }

		[DisplayName("Role")]
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}

	// TODO: document, for the read-only user view page
	public class UserDetailsViewModel
	{
		public int Id { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }

		[DisplayName("Role")]
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}

	// TODO: document, for the User/Edit screen
	public class UserEditViewModel : UserEditPostViewModel
	{
		[EmailAddress]
		public string Email { get; set; }

		public bool EmailConfirmed { get; set; }

		public bool IsLockedOut { get; set; }

		[DisplayName("Available Roles")]
		public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
	}

	/// <summary>
	/// Just the fields that can be posted from the User edit page
	/// </summary>
	public class UserEditPostViewModel
	{
		public int Id { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }

		public string OriginalUserName => UserName;

		public IEnumerable<int> SelectedRoles { get; set; } = new List<int>();

		[DisplayName("Selected Roles")]
		public string SelectedRolesStr
		{
			get => string.Join(",", SelectedRoles);
			set => SelectedRoles = value?
				.Split(",")
				.Select(int.Parse)
				.ToList() ?? new List<int>();
		}
	}
}
