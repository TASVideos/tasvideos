using TASVideos.Common;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Subforum;

[AllowAnonymous]
[RequireCurrentPermissions]
public class IndexModel(ApplicationDbContext db, IForumService forumService) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	[FromRoute]
	public int Id { get; set; }

	public ForumDisplay Forum { get; set; } = null!;
	public PageOf<ForumTopicEntry> Topics { get; set; } = new([], new());
	public Dictionary<int, (string PostsCreated, string PostsEdited)> ActivityTopics { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var forum = await db.Forums
			.ExcludeRestricted(seeRestricted)
			.Where(f => f.Id == Id)
			.Select(f => new ForumDisplay(f.Id, f.Name, f.Description, f.CanCreateTopics))
			.SingleOrDefaultAsync();

		if (forum is null)
		{
			return NotFound();
		}

		int userIdForVotes = User.GetUserId();

		Forum = forum;
		Topics = await db.ForumTopics
			.ForForum(Id)
			.Select(ft => new ForumTopicEntry
			{
				Id = ft.Id,
				Topics = ft.Title,
				Author = ft.Poster!.UserName,
				Type = ft.Type,
				IsLocked = ft.IsLocked,
				Replies = ft.ForumPosts.Count,
				LastPost = ft.ForumPosts
					.Where(fp => fp.Id == ft.ForumPosts.Max(fpp => fpp.Id))
					.Select(fp => new ForumTopicEntry.LastPostEntry
					{
						Id = fp.Id,
						PosterName = fp.Poster!.UserName,
						CreateTimestamp = fp.CreateTimestamp
					})
					.FirstOrDefault(),
				Votes = ft.Submission != null && ft.Poll != null
					&& ft.Poll.PollOptions.Any(o => o.Text == SiteGlobalConstants.PollOptionYes)
					&& ft.Poll.PollOptions.Any(o => o.Text == SiteGlobalConstants.PollOptionsMeh)
					&& ft.Poll.PollOptions.Any(o => o.Text == SiteGlobalConstants.PollOptionNo)
					? new VoteCounts
					{
						VotesYes = ft.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionYes).Votes.Count,
						VotesMeh = ft.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionsMeh).Votes.Count,
						VotesNo = ft.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionNo).Votes.Count,
						UserVotedYes = ft.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionYes).Votes.Any(v => v.UserId == userIdForVotes),
						UserVotedMeh = ft.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionsMeh).Votes.Any(v => v.UserId == userIdForVotes),
						UserVotedNo = ft.Poll.PollOptions.Single(o => o.Text == SiteGlobalConstants.PollOptionNo).Votes.Any(v => v.UserId == userIdForVotes),
					}
				: null,
			})
			.OrderByDescending(ft => ft.Type)
			.ThenByDescending(ft => ft.LastPost!.Id) // The database does not enforce it, but we can assume a topic will always have at least one post
			.PageOf(Search);

		ActivityTopics = await forumService.GetPostActivityOfSubforum(Id);

		return Page();
	}

	public record ForumDisplay(int Id, string Name, string? Description, bool CanCreateTopics);

	public class ForumTopicEntry
	{
		[TableIgnore]
		public int Id { get; init; }

		public string Topics { get; init; } = "";

		[MobileHide]
		public int Replies { get; init; }

		[MobileHide]
		public string? Author { get; init; }

		[TableIgnore]
		public ForumTopicType Type { get; init; }

		[TableIgnore]
		public bool IsLocked { get; init; }

		[TableIgnore]
		public VoteCounts? Votes { get; init; }

		[TableIgnore]
		public LastPostEntry? LastPost { get; init; }

		public class LastPostEntry
		{
			public int Id { get; init; }
			public string? PosterName { get; init; }
			public DateTime CreateTimestamp { get; init; }
		}
	}
}
