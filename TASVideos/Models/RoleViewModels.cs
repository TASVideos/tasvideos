using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a Role entry for the purpose of display
	/// </summary>
	public class RoleDisplayViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public IEnumerable<string> Permissions { get; set; } = new List<string>();
	}

	/// <summary>
	/// Represents a Role entry for the purpose of editing
	/// </summary>
	public class RoleEditViewModel
	{
		public int? Id { get; set; }

		[Required]
		[StringLength(50)]
		[Display(Name = "Role Name")]
		public string Name { get; set; }

		[Required]
		[StringLength(200)]
		public string Description { get; set; }

		[Display(Name = "Selected Permissions")]
		public IEnumerable<PermissionTo> SelectedPermisisons { get; set; } = new List<PermissionTo>();

		public string HackySelectedPermissions
		{
			get => string.Join(",", SelectedPermisisons.Select(p => (int)p));
			set => SelectedPermisisons = value?.Split(",")
				.Select(int.Parse)
				.Select(i => (PermissionTo)i)
				.ToList() ?? new List<PermissionTo>();
		}

		[Display(Name = "Available Permissions")]
		public IEnumerable<SelectListItem> AvailablePermissions { get; set; } = new List<SelectListItem>();
	}
}