using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Publications.Models;

public class PublicationCatalogModel
{
	public string Title { get; set; } = "";

	[Required]
	[Display(Name = "Rom")]
	public int RomId { get; set; }

	[Required]
	[Display(Name = "Game")]
	public int GameId { get; set; }

	[Required]
	[Display(Name = "System")]
	public int SystemId { get; set; }

	[Required]
	[Display(Name = "System Framerate")]
	public int SystemFrameRateId { get; set; }
}
