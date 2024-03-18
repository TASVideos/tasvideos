using System.ComponentModel.DataAnnotations;
using TASVideos.Core;

namespace TASVideos.Pages.Games.Models;

public class SystemPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
{
	[Display(Name = "System")]
	public string? SystemCode { get; set; }

	[Display(Name = "Starts with")]
	public string? Letter { get; set; }

	[Display(Name = "Genre")]
	public string? Genre { get; set; }

	[Display(Name = "Group")]
	public string? Group { get; set; }

	public string? SearchTerms { get; set; }

	public static new SystemPageOf<T> Empty() => new([]);
}
