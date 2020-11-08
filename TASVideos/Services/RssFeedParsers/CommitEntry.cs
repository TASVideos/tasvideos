namespace TASVideos.Services.RssFeedParsers
{
	public interface ICommitEntry
	{
		public string Author { get; }
		public string At { get; }
		public string Message { get; }
		public string Link { get; }
	}

	internal class CommitEntry : ICommitEntry
	{
		public string Author { get; set; } = "";
		public string At { get; set; } = "";
		public string Message { get; set; } = "";
		public string Link { get; set; } = "";
	}
}
