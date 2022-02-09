namespace TASVideos.ViewComponents.Models;

public class FirstEditionModel
{
	public int Id { get; init; }
	public int GameId { get; init; }
	public string Title { get; init; } = "";
	public int PublicationClassId { get; init; }
	public string? PublicationClassIconPath { get; init; } = "";
	public string PublicationClassName { get; init; } = "";
	public DateTime PublicationDate { get; init; }
}
