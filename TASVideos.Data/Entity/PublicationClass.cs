namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class PublicationClass
{
	public int Id { get; set; }

	public string Name { get; set; } = "";

	public string? IconPath { get; set; }

	public string Link { get; set; } = "";
}
