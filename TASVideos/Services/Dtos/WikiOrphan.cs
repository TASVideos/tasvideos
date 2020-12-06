using System;

namespace TASVideos.Services
{
	public class WikiOrphan
	{
		public string PageName { get; init; } = "";
		public DateTime LastUpdateTimeStamp { get; init; }
		public string? LastUpdateUserName { get; init; }
	}
}
