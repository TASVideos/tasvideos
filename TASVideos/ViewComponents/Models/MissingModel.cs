namespace TASVideos.ViewComponents.Models;

public class MissingModel
{
	public IReadOnlyCollection<Entry> Publications { get; init; } = [];
	public IReadOnlyCollection<Entry> Submissions { get; init; } = [];
	public record Entry(int Id, string Title);
}
