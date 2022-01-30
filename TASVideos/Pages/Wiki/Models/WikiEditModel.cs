using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Wiki.Models;

public class WikiEditModel
{
	public DateTime EditStart { get; set; } = DateTime.UtcNow;

	[Required]
	public string Markup { get; set; } = "";

	[Display(Name = "Minor Edit")]
	public bool MinorEdit { get; set; }

	[Display(Name = "Edit Comments", Description = "Please enter a descriptive summary of your change. Leaving this blank is discouraged.")]
	[MaxLength(500)]
	public string? RevisionMessage { get; set; }
}
