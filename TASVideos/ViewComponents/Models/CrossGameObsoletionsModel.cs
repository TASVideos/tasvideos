namespace TASVideos.ViewComponents.Models;

public class CrossGameObsoletionsModel
{
	public Dictionary<Entry, HashSet<Entry>> AllObsoletionGroups { get; init; } = new Dictionary<Entry, HashSet<Entry>>();
	public record Entry(int Id, string Title);
}
