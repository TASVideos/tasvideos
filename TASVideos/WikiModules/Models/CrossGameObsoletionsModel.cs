namespace TASVideos.WikiModules.Models;

public class CrossGameObsoletionsModel
{
	public Dictionary<Entry, HashSet<Entry>> AllObsoletionGroups { get; init; } = [];
	public record Entry(int Id, string Title);
}
