using System.ComponentModel.DataAnnotations;
using TASVideos.Core;

namespace TASVideos.Pages.Games.Models;

public class GameListModel
{
	[Sortable]
	[Display(Name = "Id")]
	public int Id { get; set; }

	[Sortable]
	[Display(Name = "Name")]
	public string DisplayName { get; set; } = "";

	// Dummy to generate column header
	[Display(Name = "Actions")]
	public object? Actions { get; set; }
}
