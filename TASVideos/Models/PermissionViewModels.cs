using System.Collections.Generic;

namespace TASVideos.Models
{
	/// <summary>
	/// Presents a Permission entry for a permission display page
	/// </summary>
	public class PermissionViewModel
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Group { get; set; }
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}
}