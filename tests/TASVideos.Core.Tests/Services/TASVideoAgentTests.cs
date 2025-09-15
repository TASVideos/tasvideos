using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class TASVideoAgentTests : TestDbBase
{
	private const string SubmissionTitle = "Test Title";
	private const int PublicationId = 1;
	private readonly Submission _submission;

	private readonly TASVideoAgent _tasVideoAgent;

	public TASVideoAgentTests()
	{
		var mockForumService = Substitute.For<IForumService>();
		_tasVideoAgent = new TASVideoAgent(_db, mockForumService);

		_db.AddForumConstantEntities();
		_submission = _db.AddSubmission().Entity;
		_submission.Title = SubmissionTitle;
		_db.SaveChanges();
	}

	[TestMethod]
	public async Task PostSubmissionTopic_CreatesPost()
	{
		await _tasVideoAgent.PostSubmissionTopic(_submission.Id, SubmissionTitle);
		var actual = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
		Assert.IsTrue(actual.Text.Contains(_submission.Id.ToString()));
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsTrue(actual.EnableBbCode);
	}

	[TestMethod]
	public async Task PostSubmissionTopic_CreatesTopic()
	{
		await _tasVideoAgent.PostSubmissionTopic(_submission.Id, SubmissionTitle);
		var actual = await _db.ForumTopics.OrderBy(fp => fp.Id).LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
		Assert.AreEqual(_submission.Id, actual.SubmissionId);
		Assert.AreEqual(SubmissionTitle, actual.Title);
		Assert.AreEqual(ForumConstants.WorkBenchForumId, actual.ForumId);
	}

	[TestMethod]
	public async Task PostSubmissionTopic_CreatesPoll()
	{
		await _tasVideoAgent.PostSubmissionTopic(_submission.Id, SubmissionTitle);
		var actual = await _db.ForumPolls.OrderBy(fp => fp.Id).LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.PollQuestion, actual.Question);
	}

	[TestMethod]
	public async Task PostSubmissionTopic_CreatesPollOptions()
	{
		await _tasVideoAgent.PostSubmissionTopic(_submission.Id, SubmissionTitle);
		var actual = await _db.ForumPollOptions.ToListAsync();

		Assert.AreEqual(3, actual.Count);
		Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionNo));
		Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionYes));
		Assert.IsTrue(actual.Any(o => o.Text == SiteGlobalConstants.PollOptionsMeh));
	}

	[TestMethod]
	public async Task PostSubmissionTopic_IdsMatch()
	{
		await _tasVideoAgent.PostSubmissionTopic(_submission.Id, SubmissionTitle);

		var post = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();
		var topic = await _db.ForumTopics.OrderBy(ft => ft.Id).LastOrDefaultAsync();
		var poll = await _db.ForumPolls.OrderBy(fp => fp.Id).LastOrDefaultAsync();
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
		await _tasVideoAgent.PostSubmissionPublished(_submission.Id, PublicationId);
		var actual = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task PostSubmissionPublished_TopicCreated()
	{
		const int topicId = 1;
		const int forumId = SiteGlobalConstants.WorkbenchForumId;
		var topic = _db.ForumTopics.Add(new ForumTopic
		{
			Id = topicId,
			ForumId = forumId,
			Title = "Title",
			SubmissionId = _submission.Id,
			PosterId = SiteGlobalConstants.TASVideoAgentId
		});
		var post = _db.ForumPosts.Add(new ForumPost
		{
			TopicId = topicId,
			ForumId = forumId,
			PosterId = SiteGlobalConstants.TASVideoAgentId
		});
		await _db.SaveChangesAsync();

		await _tasVideoAgent.PostSubmissionPublished(_submission.Id, PublicationId);
		var actual = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.PublishedMoviesForumId, topic.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.PublishedMoviesForumId, post.Entity.ForumId);
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, actual.PosterId);
		Assert.AreEqual(SiteGlobalConstants.NewPublicationPostSubject, actual.Subject);
		Assert.IsFalse(actual.EnableHtml);
		Assert.IsTrue(actual.EnableBbCode);
		Assert.IsTrue(actual.Text.Contains(PublicationId.ToString()));
	}

	[TestMethod]
	public async Task PostSubmissionUnpublish_NoTopic_DoesNotPost()
	{
		await _tasVideoAgent.PostSubmissionUnpublished(_submission.Id);
		var actual = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task PostSubmissionUnpublish_Posts()
	{
		var topic = _db.ForumTopics.Add(new ForumTopic
		{
			Title = "Title",
			SubmissionId = _submission.Id,
			ForumId = SiteGlobalConstants.PublishedMoviesForumId,
			PosterId = SiteGlobalConstants.TASVideoAgentId
		});
		await _db.SaveChangesAsync();

		await _tasVideoAgent.PostSubmissionUnpublished(_submission.Id);
		var actual = await _db.ForumPosts.OrderBy(fp => fp.Id).LastOrDefaultAsync();

		Assert.IsNotNull(actual);
		Assert.AreEqual(SiteGlobalConstants.WorkbenchForumId, topic.Entity.ForumId);
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
		var entry = _db.AddUser("_");
		await _db.SaveChangesAsync();

		await _tasVideoAgent.SendWelcomeMessage(entry.Entity.Id);

		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendWelcomeMessage_Success()
	{
		const string userName = "TestUser";
		const string template = "Welcome [[username]]";
		const string subject = "Test";
		var userEntry = _db.AddUser(userName);
		var topic = _db.AddTopic().Entity;
		_db.ForumPosts.Add(new ForumPost
		{
			Id = SiteGlobalConstants.WelcomeToTasvideosPostId,
			Text = template,
			Subject = subject,
			Topic = topic,
			Forum = topic.Forum,
			PosterId = SiteGlobalConstants.TASVideoAgentId
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
		Assert.AreEqual(subject, message.Subject);
	}

	[TestMethod]
	public async Task SendAutoAssignedRole_UserNotFound_NoMessageSent()
	{
		const int notExists = int.MaxValue;
		await _tasVideoAgent.SendAutoAssignedRole(notExists, "");

		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendAutoAssignedRole_PostNotFound_NoMessageSent()
	{
		var entry = _db.AddUser("_");
		await _db.SaveChangesAsync();

		await _tasVideoAgent.SendAutoAssignedRole(entry.Entity.Id, "");

		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendAutoAssignedRole_Success()
	{
		const string userName = "TestUser";
		const string template = "Congratulations on [[publicationTitle]]. You have been assigned [[role]]";
		const string roleName = "Test Forum User";
		const string subject = "Test";
		var userEntry = _db.AddUser(userName);
		var topic = _db.AddTopic().Entity;
		_db.ForumPosts.Add(new ForumPost
		{
			Id = SiteGlobalConstants.AutoAssignedRolePostId,
			Text = template,
			Subject = subject,
			Topic = topic,
			Forum = topic.Forum,
			PosterId = SiteGlobalConstants.TASVideoAgentId
		});
		await _db.SaveChangesAsync();

		await _tasVideoAgent.SendAutoAssignedRole(userEntry.Entity.Id, roleName);

		Assert.AreEqual(1, _db.PrivateMessages.Count());
		var message = _db.PrivateMessages.Single();
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, message.FromUserId);
		Assert.AreEqual(userEntry.Entity.Id, message.ToUserId);
		Assert.IsTrue(message.EnableBbCode);
		Assert.IsFalse(message.EnableHtml);
		Assert.IsTrue(message.Text.Contains(roleName));
		Assert.AreEqual(subject, message.Subject);
	}

	[TestMethod]
	public async Task SendPublishedAuthorRole_UserNotFound_NoMessageSent()
	{
		const int notExists = int.MaxValue;
		await _tasVideoAgent.SendPublishedAuthorRole(notExists, "", "");

		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendPublishedAuthorRole_PostNotFound_NoMessageSent()
	{
		var entry = _db.AddUser("_");
		await _db.SaveChangesAsync();

		await _tasVideoAgent.SendPublishedAuthorRole(entry.Entity.Id, "", "");

		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendPublishedAuthorRole_Success()
	{
		const string userName = "TestUser";
		const string template = "Congratulations on [[publicationTitle]]. You have been assigned [[role]]";
		const string subject = "Test";
		const string roleName = "Test Role";
		const string publicationTitle = "Test in 1:00";
		var userEntry = _db.AddUser(userName);
		var topic = _db.AddTopic().Entity;
		_db.ForumPosts.Add(new ForumPost
		{
			Id = SiteGlobalConstants.PublishedAuthorRoleAddedPostId,
			Text = template,
			Subject = subject,
			Topic = topic,
			Forum = topic.Forum,
			PosterId = SiteGlobalConstants.TASVideoAgentId
		});
		await _db.SaveChangesAsync();

		await _tasVideoAgent.SendPublishedAuthorRole(userEntry.Entity.Id, roleName, publicationTitle);

		Assert.AreEqual(1, _db.PrivateMessages.Count());
		var message = _db.PrivateMessages.Single();
		Assert.AreEqual(SiteGlobalConstants.TASVideoAgentId, message.FromUserId);
		Assert.AreEqual(userEntry.Entity.Id, message.ToUserId);
		Assert.IsTrue(message.EnableBbCode);
		Assert.IsFalse(message.EnableHtml);
		Assert.IsTrue(message.Text.Contains(roleName));
		Assert.IsTrue(message.Text.Contains(publicationTitle));
		Assert.AreEqual(subject, message.Subject);
	}
}
