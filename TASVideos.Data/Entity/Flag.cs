using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
	public class Flag
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; } = "";

		public string? IconPath { get; set; }
		public string? LinkPath { get; set; }

		[Required]
		public string Token { get; set; } = "";

		public PermissionTo? PermissionRestriction { get; set; }
	}
}
