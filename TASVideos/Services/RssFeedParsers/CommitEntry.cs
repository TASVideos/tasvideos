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
		public string Author { get; init; } = "";
		public string At { get; init; } = "";
		public string Message { get; init; } = "";
		public string Link { get; init; } = "";
	}
}
