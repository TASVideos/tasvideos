using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Services;

public interface ITASVideosGrue
{
	Task RejectAndMove(int submissionId);
}

internal class TASVideosGrue : ITASVideosGrue
{
	private readonly ApplicationDbContext _db;
	private readonly IForumService _forumService;

	private static readonly string[] RandomMessages =
	{
			"... minty!",
			"... blech, salty!",
			"... blech, bitter!",
			"... juicy!",
			"... crunchy!",
			"... sweet!",
			"... want more!",
			"... *burp*!",
			"... om, nom, nom... nom nom",
			"... 'twas dry"
		};

	public TASVideosGrue(ApplicationDbContext db, IForumService forumService)
	{
		_db = db;
		_forumService = forumService;
	}

	public async Task RejectAndMove(int submissionId)
	{
		var topic = await _db.ForumTopics.SingleOrDefaultAsync(f => f.SubmissionId == submissionId);

		// We intentionally silently fail here.
		// Otherwise we would leave submission rejection in a partial state
		// which would be worse than a missing forum post
		if (topic is not null)
		{
			topic.ForumId = SiteGlobalConstants.GrueFoodForumId;
			var entry = _db.ForumPosts.Add(new ForumPost
			{
				TopicId = topic.Id,
				ForumId = topic.ForumId,
				CreateUserName = SiteGlobalConstants.TASVideosGrue,
				LastUpdateUserName = SiteGlobalConstants.TASVideosGrue,
				PosterId = SiteGlobalConstants.TASVideosGrueId,
				Text = RejectionMessage(topic.CreateTimestamp),
				PosterMood = ForumPostMood.Normal
			});
			await _db.SaveChangesAsync();

			_forumService.CacheLatestPost(
				topic.ForumId,
				topic.Id,
				new LatestPost(entry.Entity.Id, entry.Entity.CreateTimestamp, SiteGlobalConstants.TASVideosGrue));
		}
	}

	private static string RejectionMessage(DateTime createTimeStamp)
	{
		string message = "om, nom, nom";
		message += (DateTime.UtcNow - createTimeStamp).TotalDays >= 365
			? "... blech, stale!"
			: RandomMessages.AtRandom();

		return message;
	}
}
