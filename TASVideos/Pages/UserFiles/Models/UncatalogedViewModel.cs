namespace TASVideos.Pages.UserFiles.Models;

public record UncatalogedViewModel(
	long Id,
	string FileName,
	string? SystemCode,
	DateTime UploadTimestamp,
	string Author);
