using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.GameGroups.Models;

public class GameGroupEditModel
{
	[StringLength(255)]
	public string Name { get; set; } = "";

	[StringLength(255)]
	public string? Abbreviation { get; set; }

	[StringLength(2000)]
	public string? Description { get; set; }
}
