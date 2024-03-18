namespace TASVideos.Pages.Games.Groups.Models;

public class GameListEntry
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public List<string> Systems { get; set; } = [];
	public int PublicationCount { get; set; }
	public int SubmissionsCount { get; set; }
	public string? GameResourcesPage { get; set; }

	public string SystemsString() => string.Join(", ", Systems);
}
