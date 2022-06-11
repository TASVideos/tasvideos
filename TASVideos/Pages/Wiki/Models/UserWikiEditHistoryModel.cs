using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Wiki.Models;

public class UserWikiEditHistoryModel
{
	[Display(Name = "Revision")]
	public int Revision { get; set; }

	[Display(Name = "Date")]
	public DateTime CreateTimestamp { get; set; }

	[Display(Name = "Page")]
	public string PageName { get; set; } = "";

	[Display(Name = "Minor Edit")]
	public bool MinorEdit { get; set; }

	[Display(Name = "Revision Message")]
	public string? RevisionMessage { get; set; }
}
