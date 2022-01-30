using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models;

public class PollCreateModel
{
	[Display(Name = "Question")]
	[StringLength(200, MinimumLength = 8)]
	public string? Question { get; set; }

	[Display(Name = "Days to Run for", Description = "0 or empty for a never-ending poll")]
	public int? DaysOpen { get; set; }

	[Display(Name = "Allow Multiple Selections")]
	public bool MultiSelect { get; set; }

	[Display(Name = "Options")]
	public IList<string> PollOptions { get; set; } = new List<string> { "", "" };

	public bool IsValid =>
		!string.IsNullOrWhiteSpace(Question)
		&& Question.Length <= 200
		&& OptionsAreValid;

	public bool OptionsAreValid =>
		PollOptions.Count(o => !string.IsNullOrWhiteSpace(o)) > 1
		&& PollOptions.All(o => o.Length <= 250);
}
