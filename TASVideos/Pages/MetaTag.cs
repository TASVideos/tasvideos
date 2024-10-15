namespace TASVideos.Pages;

public class MetaTag
{
	public string? Title { get; init; }
	public string? Description { get; init; }
	public string? RelativeUrl { get; init; }
	public string? Image { get; init; }
	public bool UseTwitterCard { get; init; }

	public static readonly MetaTag Default = new();
}
