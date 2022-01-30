using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class TASVideoAgentTests
{
	private const int SubmissionId = 1;
	private const string SubmissionTitle = "Test Title";
	private const int PublicationId = 1;

	private readonly ITASVideoAgent _tasVideoAgent;
	private readonly TestDbContext _db;

	public TASVideoAgentTests()
	{
		_db = TestDbContext.Create();
		var mockForumService = new Mock<IForumService>();
		_tasVideoAgent = new TASVideoAgent(_db, mockForumService.Object);
	}

	[TestMethod]
	public async Task PostSubmissionTopic_CreatesPost()
	{
		await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
		Assert.IsTrue(actual.Text.Contains(SubmissionId.ToString()));
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsTrue(actual.EnableBbCode);
	}

	[TestMethod]
	public async Task PostSubmissionTopic_CreatesTopic()
	{
		await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);
		var actual = await _db.ForumTopics.LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
		Assert.AreEqual(SubmissionId, actual.SubmissionId);
		Assert.AreEqual(SubmissionTitle, actual.Title);
		Assert.AreEqual(ForumConstants.WorkBenchForumId, actual.ForumId);
	}

	[TestMethod]
	public async Task PostSubmissionTopic_CreatesPoll()
	{
		await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);
		var actual = await _db.ForumPolls.LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
		Assert.AreEqual(SiteGlobalConstants.PollQuestion, actual.Question);
	}

	[TestMethod]
	public async Task PostSubmissionTopic_CreatesPollOptions()
	{
		await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);
		var actual = await _db.ForumPollOptions.ToListAsync();

		Assert.AreEqual(3, actual.Count);
		Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionNo));
		Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionYes));
		Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionsMeh));
	}

	[TestMethod]
	public async Task PostSubmissionTopic_IdsMatch()
	{
		await _tasVideoAgent.PostSubmissionTopic(SubmissionId, SubmissionTitle);

		var post = await _db.ForumPosts.LastOrDefaultAsync();
		var topic = await _db.ForumTopics.LastOrDefaultAsync();
		var poll = await _db.ForumPolls.LastOrDefaultAsync();
		var options = await _db.ForumPollOptions.ToListAsync();

		Assert.IsNotNull(post);
		Assert.IsNotNull(topic);
		Assert.IsNotNull(poll);

		Assert.AreEqual(topic.Id, post.TopicId);
		Assert.AreEqual(topic.Id, poll.TopicId);
		Assert.IsTrue(options.All(o => o.PollId == poll.Id));
	}

	[TestMethod]
	public async Task PostSubmissionPublished_NoTopic_DoesNotPost()
	{
		await _tasVideoAgent.PostSubmissionPublished(SubmissionId, PublicationId);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task PostSubmissionPublished_TopicCreated()
	{
		var topic = _db.ForumTopics.Add(new ForumTopic
		{
			Title = "Title",
			SubmissionId = SubmissionId
		});
		await _db.SaveChangesAsync();

		await _tasVideoAgent.PostSubmissionPublished(SubmissionId, PublicationId);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.PublishedMoviesForumId, topic.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
		Assert.AreEqual(SiteGlobalConstants.NewPublicationPostSubject, actual.Subject);
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsTrue(actual.EnableBbCode);
		Assert.IsTrue(actual.Text.Contains(PublicationId.ToString()));
	}
}
