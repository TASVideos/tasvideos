using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class TASVideosGrueTests : TestDbBase
{
	private readonly TASVideosGrue _tasVideosGrue;

	public TASVideosGrueTests()
	{
		var mockForumService = Substitute.For<IForumService>();
		_tasVideosGrue = new TASVideosGrue(_db, mockForumService);
	}

	[TestMethod]
	public async Task PostSubmissionRejection_NoTopic_DoesNotPost()
	{
		const int submissionId = 1;
		await _tasVideosGrue.RejectAndMove(submissionId);
		var actual = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task PostSubmissionRejection_TopicCreated()
	{
		_db.AddForumConstantEntities();
		var sub = _db.AddSubmission().Entity;
		var topic = _db.AddTopic().Entity;
		topic.ForumId = SiteGlobalConstants.WorkbenchForumId;
		topic.Title = "Title";
		topic.Submission = sub;
		var post = _db.ForumPosts.Add(new ForumPost
		{
			Topic = topic,
			ForumId = topic.ForumId,
			PosterId = SiteGlobalConstants.TASVideosGrueId
		});
		await _db.SaveChangesAsync();

		await _tasVideosGrue.RejectAndMove(sub.Id);
		var actual = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.GrueFoodForumId, topic.ForumId);
		Assert.AreEqual(SiteGlobalConstants.GrueFoodForumId, post.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.TASVideosGrueId, actual.PosterId);
		Assert.AreEqual(SiteGlobalConstants.GrueFoodForumId, actual.ForumId);
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsFalse(actual.EnableBbCode);
		Assert.IsFalse(actual.Text.Contains("stale"));
	}

	[TestMethod]
	public async Task PostSubmissionRejection_StaleIfOld()
	{
		_db.AddForumConstantEntities();
		var sub = _db.AddSubmission().Entity;
		var topic = _db.AddTopic().Entity;
		topic.CreateTimestamp = DateTime.UtcNow.AddYears(-1);
		topic.Title = "Title";
		topic.Submission = sub;
		await _db.SaveChangesAsync();

		await _tasVideosGrue.RejectAndMove(sub.Id);
		var actual = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.GrueFoodForumId, topic.ForumId);
		Assert.AreEqual(SiteGlobalConstants.TASVideosGrueId, actual.PosterId);
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsFalse(actual.EnableBbCode);
		Assert.IsTrue(actual.Text.Contains("stale"));
	}
}
