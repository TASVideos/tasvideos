namespace TASVideos.Core.Services;

public interface ITASVideosGrue
{
	Task RejectAndMove(int submissionId);
}

internal class TASVideosGrue(ApplicationDbContext db, IForumService forumService) : ITASVideosGrue
{
	private static readonly string[] RandomMessages =
	[
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
	];

	public async Task RejectAndMove(int submissionId)
	{
		var topic = await db.ForumTopics.SingleOrDefaultAsync(f => f.SubmissionId == submissionId);

		// We intentionally silently fail here,
		// otherwise we would leave submission rejection in a partial state
		// which would be worse than a missing forum post
		if (topic is not null)
		{
			topic.ForumId = SiteGlobalConstants.GrueFoodForumId;
			var postsToMove = await db.ForumPosts
				.ForTopic(topic.Id)
				.ToListAsync();
			foreach (var post in postsToMove)
			{
				post.ForumId = SiteGlobalConstants.GrueFoodForumId;
			}

			var entry = db.ForumPosts.Add(new ForumPost
			{
				TopicId = topic.Id,
				ForumId = topic.ForumId,
				PosterId = SiteGlobalConstants.TASVideosGrueId,
				Text = RejectionMessage(topic.CreateTimestamp),
				PosterMood = ForumPostMood.Normal
			});
			await db.SaveChangesAsync();

			forumService.CacheLatestPost(
				topic.ForumId,
				topic.Id,
				new LatestPost(entry.Entity.Id, entry.Entity.CreateTimestamp, SiteGlobalConstants.TASVideosGrue));
			forumService.ClearTopicActivityCache();
		}
	}

	private static string RejectionMessage(DateTime createTimeStamp)
	{
		const string message = "om, nom, nom";
		return message + ((DateTime.UtcNow - createTimeStamp).TotalDays >= 365
			? "... blech, stale!"
			: RandomMessages.AtRandom());
	}
}
