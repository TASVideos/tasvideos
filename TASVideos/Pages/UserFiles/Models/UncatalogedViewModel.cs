namespace TASVideos.Pages.UserFiles.Models;

public class UncatalogedViewModel
{
	public long Id { get; init; }
	public string FileName { get; init; } = "";
	public string? SystemCode { get; init; }
	public DateTime UploadTimestamp { get; init; }
	public string Author { get; init; } = "";
}
