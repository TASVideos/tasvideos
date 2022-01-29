namespace TASVideos.ViewComponents;

public class GameNameModel
{
	public int GameId { get; init; }
	public string DisplayName { get; init; } = "";

	public string? System { get; init; }

	public bool IsSystem => !string.IsNullOrWhiteSpace(System);
}
