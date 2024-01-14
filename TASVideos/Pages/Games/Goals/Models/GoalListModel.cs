using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Games.Goals.Models;

public class GoalListModel
{
	public int Id { get; set; }

	[Display(Name = "Name")]
	public string DisplayName { get; set; } = "";

	public IEnumerable<PublicationEntry> Publications { get; set; } = new List<PublicationEntry>();

	public IEnumerable<SubmissionEntry> Submissions { get; set; } = new List<SubmissionEntry>();

	public record PublicationEntry(int Id, string Title, bool Obs);

	public record SubmissionEntry(int Id, string Title);
}
