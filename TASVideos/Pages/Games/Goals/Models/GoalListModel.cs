using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Games.Goals.Models;

public class GoalListModel
{
	public int Id { get; set; }

	[Display(Name = "Name")]
	public string DisplayName { get; set; } = "";

	public IEnumerable<PublicationEntry> Publications { get; set; } = [];

	public IEnumerable<SubmissionEntry> Submissions { get; set; } = [];

	public record PublicationEntry(int Id, string Title, bool Obs);

	public record SubmissionEntry(int Id, string Title);
}
