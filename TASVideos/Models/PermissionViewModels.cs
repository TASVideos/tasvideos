using System.Collections.Generic;
using System.ComponentModel;
using TASVideos.Data.Entity;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a Permission entry for the purpose of display
	/// </summary>
	public class PermissionDisplayViewModel
	{
		[DisplayName("Permission")]
		public PermissionTo Id { get; set; }
		public string Description { get; set; }
		public string Group { get; set; }
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}
}