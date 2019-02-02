using System;
using System.Collections.Generic;
using System.ComponentModel;
using TASVideos.Data;

namespace TASVideos.Pages.Users.Models
{
	public class UserListModel
	{
		[Sortable]
		public int Id { get; set; }

		[DisplayName("User Name")]
		[Sortable]
		public string UserName { get; set; }

		[DisplayName("Role")]
		public IEnumerable<string> Roles { get; set; } = new List<string>();

		[DisplayName("Created")]
		[Sortable]
		public DateTime CreateTimeStamp { get; set; }

		// Dummy to generate column header
		public object Actions { get; set; }
	}
}
