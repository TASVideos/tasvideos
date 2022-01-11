using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Roms.Models
{
	public class RomEditModel
	{
		[Required]
		[StringLength(255)]
		[Display(Name = "Name")]
		public string Name { get; set; } = "";

		[Required]
		[RegularExpression("^[A-Fa-f0-9]*$")]
		[StringLength(32, MinimumLength = 32)]
		[Display(Name = "Md5")]
		public string Md5 { get; set; } = "";

		[Required]
		[RegularExpression("^[A-Fa-f0-9]*$")]
		[StringLength(40, MinimumLength = 40)]
		[Display(Name = "Sha1")]
		public string Sha1 { get; set; } = "";

		[StringLength(50)]
		[Display(Name = "Version")]
		public string? Version { get; set; }

		[Required]
		[StringLength(50)]
		[Display(Name = "Region")]
		public string? Region { get; set; }

		[Required]
		public RomTypes Type { get; set; }
	}
}
