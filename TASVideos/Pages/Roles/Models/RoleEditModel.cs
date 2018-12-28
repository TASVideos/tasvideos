using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using TASVideos.Models;

namespace TASVideos.Pages.Roles.Models
{
	/// <summary>
	/// Represents a Role entry for the purpose of editing
	/// </summary>
	public class RoleEditModel
	{
		public int? Id { get; set; }

		[Required]
		[StringLength(50)]
		[Display(Name = "Name")]
		public string Name { get; set; }

		[Required]
		[StringLength(200)]
		public string Description { get; set; }

		[AtLeastOne(ErrorMessage = "At least one permission is required.")]
		[Display(Name = "Selected Permissions")]
		public IEnumerable<int> SelectedPermissions { get; set; } = new List<int>();

		[Display(Name = "Selected Assignable Permissions")]
		public IEnumerable<int> SelectedAssignablePermissions { get; set; } = new List<int>();

		[Display(Name = "Related Links")]
		public IEnumerable<string> Links { get; set; } = new List<string>();
	}
}
