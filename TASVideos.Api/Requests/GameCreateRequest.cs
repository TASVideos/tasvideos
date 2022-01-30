using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591
namespace TASVideos.Api.Requests;

/// <summary>
/// Represents the filtering criteria for creating a new game.
/// </summary>
// TODO: common code for all this, same as GameEditModel
public class GameCreateRequest
{
	[Required]
	[StringLength(8)]
	public string SystemCode { get; init; } = "";

	[Required]
	[StringLength(250)]
	public string GoodName { get; init; } = "";

	[Required]
	[StringLength(100)]
	public string DisplayName { get; init; } = "";

	[Required]
	[StringLength(8)]
	public string Abbreviation { get; init; } = "";

	[Required]
	[StringLength(64)]
	public string SearchKey { get; init; } = "";

	[Required]
	[StringLength(250)]
	public string YoutubeTags { get; init; } = "";

	[StringLength(250)]
	public string? ScreenshotUrl { get; init; }

	[StringLength(300)]
	public string? GameResourcesPage { get; init; }

	[Display(Name = "Genres")]
	public IEnumerable<int> Genres { get; init; } = new List<int>();
}
