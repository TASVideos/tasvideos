namespace TASVideos.ViewComponents
{
	public class MiniMovieModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = "";
		public ScreenshotFile Screenshot { get; set; } = new();
		public string? OnlineWatchingUrl { get; set; }

		public class ScreenshotFile
		{
			public string Path { get; set; } = "";
			public string? Description { get; set; }
		}
	}
}
