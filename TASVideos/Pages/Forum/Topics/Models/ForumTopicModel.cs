using TASVideos.Core;
using TASVideos.Data.Entity.Forum;
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
	public int Id { get; set; }
	public int LastPostId { get; set; }
	public bool Restricted { get; set; }
	public bool IsWatching { get; set; }
	public bool IsLocked { get; set; }
	public string Title { get; set; } = "";
	public int ForumId { get; set; }
	public string ForumName { get; set; } = "";
	public ForumTopicType Type { get; set; }
	public int? SubmissionId { get; set; }
	public PageOf<ForumPostEntry> Posts { get; set; } = PageOf<ForumPostEntry>.Empty();
	public PollModel? Poll { get; set; }
	public int? GameId { get; set; }
	public string? GameName { get; set; }
	public int CategoryId { get; set; }

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
