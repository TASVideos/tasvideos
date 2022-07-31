using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
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
		var topicId = 1;
		var forumId = 1;
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
			ForumId = forumId,
		});
		await _db.SaveChangesAsync();

		await _tasVideoAgent.PostSubmissionPublished(SubmissionId, PublicationId);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.PublishedMoviesForumId, topic.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.PublishedMoviesForumId, post.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
		Assert.AreEqual(SiteGlobalConstants.NewPublicationPostSubject, actual.Subject);
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsTrue(actual.EnableBbCode);
		Assert.IsTrue(actual.Text.Contains(PublicationId.ToString()));
	}

	[TestMethod]
	public async Task PostSubmissionUnpublish_NoTopic_DoesNotPost()
	{
		await _tasVideoAgent.PostSubmissionUnpublished(SubmissionId);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task PostSubmissionUnpublish_Posts()
	{
		var topic = _db.ForumTopics.Add(new ForumTopic
		{
			Title = "Title",
			SubmissionId = SubmissionId
		});
		await _db.SaveChangesAsync();

		await _tasVideoAgent.PostSubmissionUnpublished(SubmissionId);
		var actual = await _db.ForumPosts.LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.WorkbenchForumId, topic.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.CreateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgent, actual.LastUpdateUserName);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
		Assert.AreEqual(SiteGlobalConstants.UnpublishSubject, actual.Subject);
		Assert.AreEqual(SiteGlobalConstants.UnpublishPost, actual.Text);
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsTrue(actual.EnableBbCode);
	}

	[TestMethod]
	public async Task SendWelcomeMessage_UserNotFound_NoMessageSent()
	{
		const int notExists = int.MaxValue;
		await _tasVideoAgent.SendWelcomeMessage(notExists);

		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendWelcomeMessage_PostNotFound_NoMessageSent()
	{
		var entry = _db.Users.Add(new User());
		await _db.SaveChangesAsync();

		await _tasVideoAgent.SendWelcomeMessage(entry.Entity.Id);

		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendWelcomeMessage_Success()
	{
		const string userName = "TestUser";
		const string template = "Welcome [[username]]";
		var userEntry = _db.Users.Add(new User { UserName = userName });
		_db.ForumPosts.Add(new ForumPost
		{
			Id = SiteGlobalConstants.WelcomeToTasvideosPostId,
			Text = template
		});
		await _db.SaveChangesAsync();

		await _tasVideoAgent.SendWelcomeMessage(userEntry.Entity.Id);

		Assert.AreEqual(1, _db.PrivateMessages.Count());
		var message = _db.PrivateMessages.Single();
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, message.FromUserId);
		Assert.AreEqual(userEntry.Entity.Id, message.ToUserId);
		Assert.IsTrue(message.EnableBbCode);
		Assert.IsFalse(message.EnableHtml);
		Assert.IsTrue(message.Text.Contains(userEntry.Entity.UserName));
	}
}
