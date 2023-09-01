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
	bool AnyVotes { get; }
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

	public bool AnyVotes => Poll?.Options.SelectMany(o => o.Voters).Any() ?? false;

	public PageOf<ForumPostEntry> Posts { get; set; } = PageOf<ForumPostEntry>.Empty();
	public PollModel? Poll { get; set; }

	public int? GameId { get; set; }
	public string? GameName { get; set; }
	public int CategoryId { get; set; }

	public class PollModel
	{
		public int PollId { get; set; }
		public string Question { get; set; } = "";
		public DateTime? CloseDate { get; set; }
		public bool MultiSelect { get; set; }
		public bool ViewPollResults { get; set; }

		public IEnumerable<PollOptionModel> Options { get; set; } = new List<PollOptionModel>();

		public class PollOptionModel
		{
			public string Text { get; set; } = "";
			public int Ordinal { get; set; }
			public IReadOnlyCollection<int> Voters { get; set; } = new List<int>();
		}
	}
}
