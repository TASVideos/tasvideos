using TASVideos.Core;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Topics.Models;

public interface IForumTopicActionBar
{
	int Id { get; }
	bool IsLocked { get; }
	bool IsWatching { get; }
	string Title { get; }
	int CategoryId { get; }
	bool Restricted { get; }
}

public class ForumTopicModel : IForumTopicActionBar
{
	public int Id { get; init; }
	public int LastPostId { get; init; }
	public bool Restricted { get; init; }
	public bool IsWatching { get; init; }
	public bool IsLocked { get; init; }
	public string Title { get; init; } = "";
	public int ForumId { get; init; }
	public string ForumName { get; init; } = "";
	public int? SubmissionId { get; init; }
	public PageOf<ForumPostEntry> Posts { get; set; } = PageOf<ForumPostEntry>.Empty();
	public PollModel? Poll { get; init; }
	public int? GameId { get; init; }
	public string? GameName { get; init; }
	public int CategoryId { get; init; }

	public class PollModel
	{
		public int PollId { get; init; }
		public string Question { get; init; } = "";
		public DateTime? CloseDate { get; init; }
		public bool MultiSelect { get; init; }
		public bool ViewPollResults { get; init; }
		public List<PollOptionModel> Options { get; set; } = [];
	}

	public record PollOptionModel(string Text, int Ordinal, List<int> Voters);
}
