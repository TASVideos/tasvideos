using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly ITopicWatcher _topicWatcher;
	private readonly IWikiPages _wikiPages;
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		var awards = Substitute.For<IAwards>();
		awards.ForUser(Arg.Any<int>()).Returns([]);
		var forumService = Substitute.For<IForumService>();
		forumService.GetPostActivityOfSubforum(Arg.Any<int>()).Returns([]);
		var pointsService = Substitute.For<IPointsService>();
		_topicWatcher = Substitute.For<ITopicWatcher>();
		_wikiPages = Substitute.For<IWikiPages>();

		_model = new IndexModel(
			_db,
			awards,
			forumService,
			pointsService,
			_topicWatcher,
			_wikiPages,
			new NullMetrics())
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentTopic_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnGet();
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_ExistingTopic_PopulatesTopicDisplay()
	{
		var user = _db.AddUser("Test User").Entity;
		var topic = _db.AddTopic().Entity;
		_db.CreatePostForTopic(topic);
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, []);
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(topic.Id, _model.Topic.Id);
		Assert.AreEqual(topic.Title, _model.Topic.Title);
		Assert.AreEqual(topic.Forum!.Name, _model.Topic.ForumName);
		await _topicWatcher.Received(1).MarkSeen(topic.Id, user.Id);
	}

	[TestMethod]
	public async Task OnGet_TopicWithSubmission_SetsSubmissionData()
	{
		var topic = _db.AddTopic().Entity;
		_db.CreatePostForTopic(topic);
		topic.Submission = _db.AddSubmission().Entity;
		await _db.SaveChangesAsync();
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.Topic.SubmissionId.HasValue);
		await _wikiPages.Received(1).Page(Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnGet_TopicWithPoll_PopulatesPollData()
	{
		var topic = _db.AddTopic().Entity;
		_db.CreatePostForTopic(topic);
		var poll = _db.CreatePollForTopic(topic).Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsNotNull(_model.Topic.Poll);
		Assert.AreEqual(poll.Question, _model.Topic.Poll.Question);
		Assert.AreEqual(poll.MultiSelect, _model.Topic.Poll.MultiSelect);
	}

	[TestMethod]
	public async Task OnGet_WithHighlightedPost_SetsHighlightedPost()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;
		_model.Search.Highlight = post.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsNotNull(_model.HighlightedPost);
		Assert.IsTrue(_model.HighlightedPost.Highlight);
	}

	[TestMethod]
	public async Task OnPostVote_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("Voter").Entity;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnPostVote(1, [1]);

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPostVote_NonExistentPoll_ReturnsNotFound()
	{
		var user = _db.AddUser("Voter").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.VoteInPolls]);

		var result = await _model.OnPostVote(999, [1]);

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPostVote_ClosedPoll_ReturnsRedirectWithError()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		var poll = _db.CreatePollForTopic(topic, isClosed: true).Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;
		AddAuthenticatedUser(_model, user, [PermissionTo.VoteInPolls]);

		var result = await _model.OnPostVote(poll.Id, [1]);

		AssertRedirect(result, "Index");
	}

	[TestMethod]
	public async Task OnPostVote_ValidVote_AddsVoteToDatabase()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		var poll = _db.CreatePollForTopic(topic).Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;
		AddAuthenticatedUser(_model, user, [PermissionTo.VoteInPolls]);

		var result = await _model.OnPostVote(poll.Id, [1]);

		AssertRedirect(result, "Index");

		var vote = await _db.ForumPollOptionVotes.FirstOrDefaultAsync();
		Assert.IsNotNull(vote);
		Assert.AreEqual(user.Id, vote.UserId);
	}

	[TestMethod]
	public async Task OnPostLock_NoPermission_ReturnsAccessDenied()
	{
		var result = await _model.OnPostLock("Test Topic", true, _publisher);
		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPostLock_NonExistentTopic_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.LockTopics]);
		_model.Id = 999;
		var result = await _model.OnPostLock("Test Topic", true, _publisher);
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPostLock_ValidTopic_UpdatesLockStatus()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();
		_model.Id = topic.Id;
		AddAuthenticatedUser(_model, user, [PermissionTo.LockTopics]);

		var result = await _model.OnPostLock("Test Topic", true, _publisher);

		AssertRedirect(result, "Index");
		await _db.Entry(topic).ReloadAsync();
		Assert.IsTrue(topic.IsLocked);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnGetWatch_NotLoggedIn_ReturnsAccessDenied()
	{
		var result = await _model.OnGetWatch();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGetWatch_LoggedIn_CallsTopicWatcher()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnGetWatch();

		AssertRedirect(result, "Index");
		await _topicWatcher.Received(1).WatchTopic(topic.Id, user.Id, Arg.Any<bool>());
	}

	[TestMethod]
	public async Task OnGetUnwatch_NotLoggedIn_ReturnsAccessDenied()
	{
		var result = await _model.OnGetUnwatch();
		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGetUnwatch_LoggedIn_CallsTopicWatcher()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();
		_model.Id = topic.Id;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnGetUnwatch();

		AssertRedirect(result, "Index");
		await _topicWatcher.Received(1).UnwatchTopic(topic.Id, user.Id);
	}

	[TestMethod]
	public void PostEntry_GetCurrentAvatar_WithMood_ReturnsMoodAvatar()
	{
		var post = new IndexModel.PostEntry
		{
			PosterMood = ForumPostMood.Angry,
			PosterMoodUrlBase = "avatar_$.png",
			PosterAvatar = "default.png"
		};

		var avatar = post.GetCurrentAvatar();

		Assert.AreEqual("avatar_2.png", avatar);
	}

	[TestMethod]
	public void PostEntry_GetCurrentAvatar_WithoutMood_ReturnsDefaultAvatar()
	{
		var post = new IndexModel.PostEntry
		{
			PosterMood = ForumPostMood.None,
			PosterAvatar = "default.png"
		};

		var avatar = post.GetCurrentAvatar();

		Assert.AreEqual("default.png", avatar);
	}

	[TestMethod]
	public void PostEntry_CalculatedRoles_BannedUser_ReturnsBannedUser()
	{
		var post = new IndexModel.PostEntry
		{
			PosterIsBanned = true,
			PosterRoles = ["Admin", "Moderator"],
			PosterPlayerRank = "Expert"
		};

		var roles = post.CalculatedRoles;

		Assert.AreEqual("Banned User", roles);
	}

	[TestMethod]
	public void PostEntry_CalculatedRoles_WithRolesAndRank_ReturnsFormattedString()
	{
		var post = new IndexModel.PostEntry
		{
			PosterIsBanned = false,
			PosterRoles = ["Moderator", "Admin"],
			PosterPlayerRank = "Expert"
		};

		var roles = post.CalculatedRoles;

		Assert.AreEqual("Admin, Moderator, Expert", roles);
	}
}
