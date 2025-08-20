using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class PollResultsModelTests : BasePageModelTests
{
	private readonly PollResultsModel _model;

	public PollResultsModelTests()
	{
		_model = new PollResultsModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentPoll_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_PollWithoutVotes_ReturnsPollWithEmptyVotes()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		await _db.SaveChangesAsync();

		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.TopicId = topic.Id;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeePollResults]);
		_model.Id = poll.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Topic", _model.Poll.TopicTitle);
		Assert.AreEqual(topic.Id, _model.Poll.TopicId);
		Assert.AreEqual("Did you like watching this movie? ", _model.Poll.Question);
		Assert.AreEqual(0, _model.Poll.Votes.Count);
	}

	[TestMethod]
	public async Task OnGet_PollWithVotes_ReturnsPollWithVoteDetails()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter1 = _db.AddUser("Voter1").Entity;
		var voter2 = _db.AddUser("Voter2").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Voting Topic";
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.TopicId = topic.Id;
		poll.Question = "Which option do you prefer?";

		var option1 = poll.PollOptions.First(o => o.Ordinal == 1);
		var option2 = poll.PollOptions.First(o => o.Ordinal == 2);

		var vote1 = new ForumPollOptionVote
		{
			PollOption = option1,
			User = voter1,
			CreateTimestamp = DateTime.UtcNow.AddMinutes(-30),
			IpAddress = "192.168.1.1"
		};

		var vote2 = new ForumPollOptionVote
		{
			PollOption = option2,
			User = voter2,
			CreateTimestamp = DateTime.UtcNow.AddMinutes(-15),
			IpAddress = "192.168.1.2"
		};

		_db.ForumPollOptionVotes.AddRange(vote1, vote2);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeePollResults]);
		_model.Id = poll.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Voting Topic", _model.Poll.TopicTitle);
		Assert.AreEqual("Which option do you prefer?", _model.Poll.Question);
		Assert.AreEqual(2, _model.Poll.Votes.Count);

		// Verify vote details
		var resultVote1 = _model.Poll.Votes.First(v => v.UserId == voter1.Id);
		Assert.AreEqual("Voter1", resultVote1.UserName);
		Assert.AreEqual(1, resultVote1.Ordinal);
		Assert.AreEqual("Yes", resultVote1.OptionText);
		Assert.AreEqual("192.168.1.1", resultVote1.IpAddress);

		var resultVote2 = _model.Poll.Votes.First(v => v.UserId == voter2.Id);
		Assert.AreEqual("Voter2", resultVote2.UserName);
		Assert.AreEqual(2, resultVote2.Ordinal);
		Assert.AreEqual("No", resultVote2.OptionText);
		Assert.AreEqual("192.168.1.2", resultVote2.IpAddress);
	}

	[TestMethod]
	public async Task OnGet_PollWithMultipleVotesFromSameUser_ReturnsAllVotes()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter = _db.AddUser("MultiVoter").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.TopicId = topic.Id;
		poll.MultiSelect = true;

		var option1 = poll.PollOptions.First(o => o.Ordinal == 1);
		var option2 = poll.PollOptions.First(o => o.Ordinal == 2);

		var vote1 = new ForumPollOptionVote
		{
			PollOption = option1,
			User = voter,
			CreateTimestamp = DateTime.UtcNow.AddMinutes(-30),
			IpAddress = "192.168.1.100"
		};

		var vote2 = new ForumPollOptionVote
		{
			PollOption = option2,
			User = voter,
			CreateTimestamp = DateTime.UtcNow.AddMinutes(-15),
			IpAddress = "192.168.1.100"
		};

		_db.ForumPollOptionVotes.AddRange(vote1, vote2);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeePollResults]);
		_model.Id = poll.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(2, _model.Poll.Votes.Count);
		Assert.IsTrue(_model.Poll.Votes.All(v => v.UserId == voter.Id));
		Assert.IsTrue(_model.Poll.Votes.All(v => v.UserName == "MultiVoter"));
	}

	[TestMethod]
	public async Task OnPostResetVote_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter = _db.AddUser("Voter").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.TopicId = topic.Id;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeePollResults]); // No ResetPollResults permission
		_model.Id = poll.Id;

		var result = await _model.OnPostResetVote(voter.Id);

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Account/AccessDenied", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPostResetVote_NonExistentPoll_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.ResetPollResults]);
		_model.Id = 999;

		var result = await _model.OnPostResetVote(123);

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPostResetVote_ValidRequest_RemovesUserVotes()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter1 = _db.AddUser("Voter1").Entity;
		var voter2 = _db.AddUser("Voter2").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.TopicId = topic.Id;

		var option1 = poll.PollOptions.First(o => o.Ordinal == 1);
		var option2 = poll.PollOptions.First(o => o.Ordinal == 2);

		var vote1 = new ForumPollOptionVote { PollOption = option1, User = voter1 };
		var vote2 = new ForumPollOptionVote { PollOption = option2, User = voter1 };
		var vote3 = new ForumPollOptionVote { PollOption = option1, User = voter2 };

		_db.ForumPollOptionVotes.AddRange(vote1, vote2, vote3);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.ResetPollResults]);
		_model.Id = poll.Id;

		var result = await _model.OnPostResetVote(voter1.Id);

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("PollResults", redirect.PageName);
		Assert.AreEqual(poll.Id, redirect.RouteValues!["Id"]);

		// Verify only voter1's votes were removed
		var remainingVotes = await _db.ForumPollOptionVotes.ToListAsync();
		Assert.AreEqual(1, remainingVotes.Count);
		Assert.AreEqual(voter2.Id, remainingVotes[0].UserId);
	}

	[TestMethod]
	public async Task OnPostResetVote_UserWithNoVotes_CompletesSuccessfully()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter = _db.AddUser("Voter").Entity;
		var userWithoutVotes = _db.AddUser("NoVotes").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.TopicId = topic.Id;

		// Add vote only from one user
		var option1 = poll.PollOptions.First();
		var vote = new ForumPollOptionVote { PollOption = option1, User = voter };
		_db.ForumPollOptionVotes.Add(vote);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.ResetPollResults]);
		_model.Id = poll.Id;

		var result = await _model.OnPostResetVote(userWithoutVotes.Id);

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		// Verify original vote is still there
		var votes = await _db.ForumPollOptionVotes.ToListAsync();
		Assert.AreEqual(1, votes.Count);
		Assert.AreEqual(voter.Id, votes[0].UserId);
	}

	[TestMethod]
	public async Task OnPostResetVote_MultipleVotesFromSameUser_RemovesAllUserVotes()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter = _db.AddUser("MultiVoter").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.TopicId = topic.Id;
		poll.MultiSelect = true;

		// Add multiple votes from same user
		var option1 = poll.PollOptions.First(o => o.Ordinal == 1);
		var option2 = poll.PollOptions.First(o => o.Ordinal == 2);

		var vote1 = new ForumPollOptionVote { PollOption = option1, User = voter };
		var vote2 = new ForumPollOptionVote { PollOption = option2, User = voter };

		_db.ForumPollOptionVotes.AddRange(vote1, vote2);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.ResetPollResults]);
		_model.Id = poll.Id;

		var result = await _model.OnPostResetVote(voter.Id);

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		// Verify all votes from user were removed
		var remainingVotes = await _db.ForumPollOptionVotes.ToListAsync();
		Assert.AreEqual(0, remainingVotes.Count);
	}

	[TestMethod]
	public async Task OnGet_PollWithVotesOrderedByOrdinal_ReturnsVotesInCorrectOrder()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter1 = _db.AddUser("Voter1").Entity;
		var voter2 = _db.AddUser("Voter2").Entity;
		var voter3 = _db.AddUser("Voter3").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.TopicId = topic.Id;

		// Ensure we have options with different ordinals
		var option1 = poll.PollOptions.First(o => o.Ordinal == 1);
		var option2 = poll.PollOptions.First(o => o.Ordinal == 2);

		// Add votes in mixed order
		var voteForOption2 = new ForumPollOptionVote { PollOption = option2, User = voter1 };
		var voteForOption1First = new ForumPollOptionVote { PollOption = option1, User = voter2 };
		var voteForOption1Second = new ForumPollOptionVote { PollOption = option1, User = voter3 };

		_db.ForumPollOptionVotes.AddRange(voteForOption2, voteForOption1First, voteForOption1Second);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeePollResults]);
		_model.Id = poll.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(3, _model.Poll.Votes.Count);

		// Verify votes are ordered by ordinal (option 1 votes first, then option 2 votes)
		var ordinals = _model.Poll.Votes.Select(v => v.Ordinal).ToList();
		var expectedOrdinals = new[] { 1, 1, 2 };
		CollectionAssert.AreEqual(expectedOrdinals, ordinals);
	}
}
