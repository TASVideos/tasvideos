namespace TASVideos.ViewComponents.Models;

public class MissingModel
{
	public IReadOnlyCollection<Entry> Publications { get; init; } = new List<Entry>();
	public IReadOnlyCollection<Entry> Submissions { get; init; } = new List<Entry>();
	public record Entry(int Id, string Title);
}
