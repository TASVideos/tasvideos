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


	/// <summary>
	/// Represents a <see cref="TASVideos.Data.Entity.User"/> for the purpose of viewing
	/// </summary>
	public class UserDetailsViewModel
	{
		public int Id { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }

		[DisplayName("Lock out Status")]
		public bool IsLockedOut { get; set; }

		public string Email { get; set; }

		[DisplayName("Email Confirmed")]
		public bool EmailConfirmed { get; set; }

		[DisplayName("Current Roles")]
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}

	/// <summary>
	/// Represents a <see cref="TASVideos.Data.Entity.User"/> for the purpose of editing
	/// </summary>
	public class UserEditViewModel : UserEditPostViewModel
	{
		[EmailAddress]
		public string Email { get; set; }

		public bool EmailConfirmed { get; set; }

		public bool IsLockedOut { get; set; }

		public string OriginalUserName => UserName;

		[DisplayName("Available Roles")]
		public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
	}

	/// <summary>
	/// Just the fields that can be posted from the <see cref="TASVideos.Data.Entity.User" /> edit page
	/// </summary>
	public class UserEditPostViewModel
	{
		public int Id { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }

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
