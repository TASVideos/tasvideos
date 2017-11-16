using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a Permission entry for the purpose of display
	/// </summary>
	public class PermissionDisplayViewModel
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Group { get; set; }
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}

	/// <summary>
	/// Represents a Permission entry for the purpose of editing
	/// </summary>
	public class PermissionEditViewModel
	{
		[Required]
		public PermissionTo Id { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		[Required]
		[StringLength(200)]
		public string Description { get; set; }

		[Required]
		[StringLength(20)]
		public string Group { get; set; }
	}
}