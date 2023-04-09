using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.UserFiles.Models;

public class CatalogViewModel
{
	public long Id { get; set; }

	[Required]
	[Display(Name = "System")]
	public int? SystemId { get; set; }

	[Display(Name = "Game")]
	public int? GameId { get; set; }

	public string Filename { get; set; } = "";
	public string AuthorName { get; set; } = "";
}
