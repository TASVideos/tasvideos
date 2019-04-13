namespace TASVideos.Pages.Publications.Models
{
	public class PublicationFileEditModel
	{
		public string Title { get; set; }

		public ScreenshotFile Screenshot { get; set; } = new ScreenshotFile();

		public class ScreenshotFile
		{
			public string Path { get; set; }
			public string Description { get; set; }
		}
	}
}
