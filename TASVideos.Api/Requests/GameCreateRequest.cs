using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591
namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents the filtering criteria for creating a new game.
	/// </summary>
	// TODO: common code for all this, same as GameEditModel
	public class GameCreateRequest
	{
		[Required]
		[Display(Name = "System")]
		public string SystemCode { get; init; } = "";

		[Required]
		[Display(Name = "Good Name")]
		[StringLength(250)]
		public string GoodName { get; init; } = "";

		[Required]
		[StringLength(100)]
		[Display(Name = "Display Name")]
		public string DisplayName { get; init; } = "";

		[Required]
		[StringLength(8)]
		[Display(Name = "Abbreviation")]
		public string Abbreviation { get; init; } = "";

		[Required]
		[StringLength(64)]
		[Display(Name = "Search Key")]
		public string SearchKey { get; init; } = "";

		[Required]
		[StringLength(250)]
		[Display(Name = "Youtube Tags")]
		public string YoutubeTags { get; init; } = "";

		[StringLength(250)]
		[Display(Name = "Screenshot Url")]
		public string? ScreenshotUrl { get; init; }

		[StringLength(300)]
		[Display(Name = "Game Resources Page")]
		public string? GameResourcesPage { get; init; }

		[Display(Name = "Genres")]
		public IEnumerable<int> Genres { get; init; } = new List<int>();
	}
}
