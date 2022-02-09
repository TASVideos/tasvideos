namespace TASVideos.Core.Services;

public record WikiOrphan(
	string PageName,
	DateTime LastUpdateTimestamp,
	string? LastUpdateUserName);
