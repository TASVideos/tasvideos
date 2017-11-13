using System.Collections.Generic;
using System.ComponentModel;

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
	public class UserEditViewModel
	{
		public int Id { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }
	}
}
