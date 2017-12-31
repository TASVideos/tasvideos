namespace TASVideos.ViewComponents
{
	public class YoutubeModel
	{
		public string Code { get; set; }
		public int Width { get; set; } = 425;
		public int Height { get; set; } = 370;
		public string Align { get; set; } = "";
		public int Start { get; set; }
		public int? Loop { get; set; }
		public bool HideLink { get; set; }
		public bool FlashBlock { get; set; }
	}
}
