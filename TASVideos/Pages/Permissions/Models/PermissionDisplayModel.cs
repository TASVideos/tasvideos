using System.Collections.Generic;
using System.ComponentModel;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Permissions.Models
{
	/// <summary>
	/// Represents a Permission entry for the purpose of display
	/// </summary>
	public class PermissionDisplayModel
	{
		public PermissionTo Id { get; set; }

		[DisplayName("Permission")]
		public string Name { get; set; }

		public string Description { get; set; }
		public string Group { get; set; }
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}
}