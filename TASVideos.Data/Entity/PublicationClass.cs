namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class PublicationClass
{
	public int Id { get; set; }

	[StringLength(20)]
	public string Name { get; set; } = "";

	[StringLength(100)]
	public string? IconPath { get; set; }

	[StringLength(100)]
	public string Link { get; set; } = "";
}
