using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class ForumWriterHelperTests : TestDbBase
{
	private readonly ForumWriterHelper _forumWriterHelper;

	public ForumWriterHelperTests()
	{
		_forumWriterHelper = new ForumWriterHelper(_db);
	}

	[TestMethod]
	public async Task GetMovieTitle_ExistingPublication_ReturnsFormattedTitle()
	{
		const string title = "SMB in 4:57:31";
		var publication = _db.AddPublication().Entity;
		publication.Title = title;
		await _db.SaveChangesAsync();
		var expectedResult = $"[{publication.Id}] {title}";

		var result = await _forumWriterHelper.GetMovieTitle(publication.Id);

		Assert.IsNotNull(result);
		Assert.AreEqual(expectedResult, result);
	}

	[TestMethod]
	public async Task GetMovieTitle_NonExistingPublication_ReturnsNull()
	{
		const int nonExistentId = 999;

		var result = await _forumWriterHelper.GetMovieTitle(nonExistentId);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetSubmissionTitle_ExistingSubmission_ReturnsTitle()
	{
		const int submissionId = 456;
		const string title = "Test Submission Title";
		var user = _db.AddUser(1).Entity;
		_db.Add(new Submission { Id = submissionId, Title = title, Submitter = user });
		await _db.SaveChangesAsync();

		var result = await _forumWriterHelper.GetSubmissionTitle(submissionId);

		Assert.IsNotNull(result);
		Assert.AreEqual(title, result);
	}

	[TestMethod]
	public async Task GetSubmissionTitle_NonExistingSubmission_ReturnsNull()
	{
		const int nonExistentId = 999;

		var result = await _forumWriterHelper.GetSubmissionTitle(nonExistentId);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetGameTitle_ExistingGame_ReturnsDisplayName()
	{
		const int gameId = 789;
		const string displayName = "Super Mario World";
		_db.Add(new Game { Id = gameId, DisplayName = displayName });
		await _db.SaveChangesAsync();

		var result = await _forumWriterHelper.GetGameTitle(gameId);

		Assert.IsNotNull(result);
		Assert.AreEqual(displayName, result);
	}

	[TestMethod]
	public async Task GetGameTitle_NonExistingGame_ReturnsNull()
	{
		const int nonExistentId = 999;

		var result = await _forumWriterHelper.GetGameTitle(nonExistentId);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetGameGroupTitle_ExistingGameGroup_ReturnsName()
	{
		const int gameGroupId = 321;
		const string name = "Mario";
		_db.Add(new GameGroup { Id = gameGroupId, Name = name });
		await _db.SaveChangesAsync();

		var result = await _forumWriterHelper.GetGameGroupTitle(gameGroupId);

		Assert.IsNotNull(result);
		Assert.AreEqual(name, result);
	}

	[TestMethod]
	public async Task GetGameGroupTitle_NonExistingGameGroup_ReturnsNull()
	{
		const int nonExistentId = 999;

		var result = await _forumWriterHelper.GetGameGroupTitle(nonExistentId);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetTopicTitle_ExistingTopic_ReturnsTitle()
	{
		const int topicId = 654;
		const string title = "Discussing Overly Pedantic Things";
		var user = _db.AddUser(1).Entity;
		var forumCategory = _db.Add(new ForumCategory { Id = 1 }).Entity;
		var forum = _db.Add(new Forum { Id = 1, Category = forumCategory }).Entity;
		_db.Add(new ForumTopic { Id = topicId, Title = title, Forum = forum, Poster = user });
		await _db.SaveChangesAsync();

		var result = await _forumWriterHelper.GetTopicTitle(topicId);

		Assert.IsNotNull(result);
		Assert.AreEqual(title, result);
	}

	[TestMethod]
	public async Task GetTopicTitle_NonExistingTopic_ReturnsNull()
	{
		const int nonExistentId = 999;

		var result = await _forumWriterHelper.GetTopicTitle(nonExistentId);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetMovieTitle_WithSpecialCharacters_ReturnsCorrectlyFormattedTitle()
	{
		const string title = "Pokémon: Red & Blue - 100% TAS";
		var publication = _db.AddPublication().Entity;
		publication.Title = title;
		await _db.SaveChangesAsync();
		var expectedResult = $"[{publication.Id}] {title}";

		var result = await _forumWriterHelper.GetMovieTitle(publication.Id);

		Assert.IsNotNull(result);
		Assert.AreEqual(expectedResult, result);
	}

	[TestMethod]
	public async Task GetSubmissionTitle_WithEmptyTitle_ReturnsEmptyString()
	{
		const int submissionId = 200;
		const string title = "";
		var user = _db.AddUser(2).Entity;
		_db.Add(new Submission { Id = submissionId, Title = title, Submitter = user });
		await _db.SaveChangesAsync();

		var result = await _forumWriterHelper.GetSubmissionTitle(submissionId);

		Assert.IsNotNull(result);
		Assert.AreEqual("", result);
	}
}
