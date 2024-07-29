using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class TASVideosGrueTests : TestDbBase
{
	private const int SubmissionId = 1;

	private readonly TASVideosGrue _tasVideosGrue;

	public TASVideosGrueTests()
	{
		var mockForumService = Substitute.For<IForumService>();
		_tasVideosGrue = new TASVideosGrue(_db, mockForumService);
	}

	[TestMethod]
	public async Task PostSubmissionRejection_NoTopic_DoesNotPost()
	{
		await _tasVideosGrue.RejectAndMove(SubmissionId);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task PostSubmissionRejection_TopicCreated()
	{
		const int topicId = 1;
		const int forumId = 1;
		var topic = _db.ForumTopics.Add(new ForumTopic
		{
			Id = topicId,
			ForumId = forumId,
			Title = "Title",
			SubmissionId = SubmissionId
		});
		var post = _db.ForumPosts.Add(new ForumPost
		{
			TopicId = topicId,
			ForumId = forumId
		});
		await _db.SaveChangesAsync();

		await _tasVideosGrue.RejectAndMove(SubmissionId);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.GrueFoodForumId, topic.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.GrueFoodForumId, post.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.TASVideosGrueId, actual.PosterId);
		Assert.AreEqual(SiteGlobalConstants.GrueFoodForumId, actual.ForumId);
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsFalse(actual.EnableBbCode);
		Assert.IsNotNull(actual.Text);
		Assert.IsFalse(actual.Text.Contains("stale"));
	}

	[TestMethod]
	public async Task PostSubmissionRejection_StaleIfOld()
	{
		var topic = _db.ForumTopics.Add(new ForumTopic
		{
			CreateTimestamp = DateTime.UtcNow.AddYears(-1),
			Title = "Title",
			SubmissionId = SubmissionId
		});
		await _db.SaveChangesAsync();

		await _tasVideosGrue.RejectAndMove(SubmissionId);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.GrueFoodForumId, topic.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.TASVideosGrueId, actual.PosterId);
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsFalse(actual.EnableBbCode);
		Assert.IsNotNull(actual.Text);
		Assert.IsTrue(actual.Text.Contains("stale"));
	}
}
