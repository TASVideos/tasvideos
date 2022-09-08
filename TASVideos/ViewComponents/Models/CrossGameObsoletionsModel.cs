namespace TASVideos.ViewComponents.Models;

public class CrossGameObsoletionsModel
{
	public Dictionary<Entry, HashSet<Entry>> AllObsoletionGroups { get; init; } = new();
	public record Entry(int Id, string Title);
}
