using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Versions.Models;

public class RomEditModel
{
	[Required]
	[Display(Name = "System")]
	[StringLength(8)]
	public string SystemCode { get; set; } = "";

	[Required]
	[StringLength(255)]
	[Display(Name = "Name")]
	public string Name { get; set; } = "";

	[RegularExpression("^[A-Fa-f0-9]*$")]
	[StringLength(32, MinimumLength = 32)]
	[Display(Name = "Md5")]
	public string? Md5 { get; set; }

	[RegularExpression("^[A-Fa-f0-9]*$")]
	[StringLength(40, MinimumLength = 40)]
	[Display(Name = "Sha1")]
	public string? Sha1 { get; set; }

	[StringLength(50)]
	[Display(Name = "Version")]
	public string? Version { get; set; }

	[Required]
	[StringLength(50)]
	[Display(Name = "Region")]
	public string? Region { get; set; }

	[Required]
	public VersionTypes Type { get; set; }

	[StringLength(255)]
	[Display(Name = "Title Override")]
	public string? TitleOverride { get; set; }
}
