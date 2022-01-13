namespace TASVideos.ViewComponents
{
	public class MiniMovieModel
	{
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public string Branch { get; init; } = "";
		public ScreenshotFile Screenshot { get; init; } = new ();
		public string? OnlineWatchingUrl { get; init; }

		public class ScreenshotFile
		{
			public string Path { get; init; } = "";
			public string? Description { get; init; }
		}
	}
}
