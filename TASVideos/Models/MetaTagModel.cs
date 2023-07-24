namespace TASVideos.Models;

public class MetaTagModel
{
	public string? Title { get; init; }
	public string? Description { get; init; }
	public string? RelativeUrl { get; init; }
	public string? Image { get; init; }
	public bool UseXCard { get; init; }

	public static readonly MetaTagModel Default = new();
}
