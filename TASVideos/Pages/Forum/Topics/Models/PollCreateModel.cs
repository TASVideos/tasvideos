using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models;

public class PollCreateModel
{
	[Display(Name = "Question")]
	[StringLength(200, MinimumLength = 8)]
	public string? Question { get; init; }

	[Display(Name = "Days to Run for", Description = "0 or empty for a never-ending poll")]
	[Range(0, 365)]
	public int? DaysOpen { get; init; }

	[Display(Name = "Allow Multiple Selections")]
	public bool MultiSelect { get; init; }

	[Display(Name = "Options")]
	public List<string> PollOptions { get; init; } = ["", ""];

	public bool IsValid =>
		!string.IsNullOrWhiteSpace(Question)
		&& Question.Length <= 200
		&& OptionsAreValid;

	public bool OptionsAreValid =>
		PollOptions.Count(o => !string.IsNullOrWhiteSpace(o)) > 1
		&& PollOptions.All(o => o.Length <= 250);

	public bool HasAnyField => !string.IsNullOrWhiteSpace(Question)
		|| DaysOpen.HasValue
		|| PollOptions.Any(o => !string.IsNullOrWhiteSpace(o));
}
