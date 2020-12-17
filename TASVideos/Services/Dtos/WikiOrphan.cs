using System;

namespace TASVideos.Services
{
	public record WikiOrphan(
		string PageName,
		DateTime LastUpdateTimeStamp,
		string? LastUpdateUserName);
}
