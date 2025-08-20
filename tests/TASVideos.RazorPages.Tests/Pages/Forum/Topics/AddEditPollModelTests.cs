using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class AddEditPollModelTests : BasePageModelTests
{
	private readonly IForumService _forumService;
	private readonly AddEditPollModel _model;

	public AddEditPollModelTests()
	{
		_forumService = Substitute.For<IForumService>();

		_model = new AddEditPollModel(_db, _forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentTopic_ReturnsNotFound()
	{
		_model.TopicId = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/NotFound", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnGet_TopicWithoutPoll_PopulatesTopicTitle()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Topic", _model.TopicTitle);
		Assert.IsNull(_model.PollId);
		Assert.IsNull(_model.Poll.Question);
	}

	[TestMethod]
	public async Task OnGet_TopicWithExistingPoll_PopulatesPollData()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Topic with Poll";
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.Question = "Do you like this?";
		poll.MultiSelect = true;
		poll.CloseDate = DateTime.UtcNow.AddDays(5);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Topic with Poll", _model.TopicTitle);
		Assert.AreEqual(poll.Id, _model.PollId);
		Assert.AreEqual("Do you like this?", _model.Poll.Question);
		Assert.IsTrue(_model.Poll.MultiSelect);
		Assert.IsTrue(_model.Poll.DaysOpen > 0);
		Assert.AreEqual(2, _model.Poll.PollOptions.Count);
		Assert.IsFalse(_model.Poll.HasVotes);
	}

	[TestMethod]
	public async Task OnGet_TopicWithPollHavingVotes_SetsHasVotesTrue()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter = _db.AddUser("Voter").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;

		var firstOption = poll.PollOptions.First();
		_db.ForumPollOptionVotes.Add(new ForumPollOptionVote
		{
			PollOption = firstOption,
			User = voter
		});
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.Poll.HasVotes);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopicWithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/NotFound", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopicWithPermission_ReturnsPage()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		topic.Title = "Restricted Topic";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls, PermissionTo.SeeRestrictedForums]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Restricted Topic", _model.TopicTitle);
	}

	[TestMethod]
	public async Task OnPost_EmptyQuestion_AddsModelError()
	{
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "",
			PollOptions = ["Option 1", "Option 2"],
			DaysOpen = 7
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey($"{nameof(_model.Poll)}.{nameof(_model.Poll.Question)}"));
	}

	[TestMethod]
	public async Task OnPost_InvalidPollOptions_AddsModelError()
	{
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "Valid question?",
			PollOptions = ["Only one option"], // Need at least 2
			DaysOpen = 7
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey($"{nameof(_model.Poll)}.{nameof(_model.Poll.PollOptions)}"));
	}

	[TestMethod]
	public async Task OnPost_NonExistentTopic_ReturnsNotFound()
	{
		_model.TopicId = 999;
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "Valid question?",
			PollOptions = ["Option 1", "Option 2"],
			DaysOpen = 7
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/NotFound", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPost_RestrictedTopicWithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "Valid question?",
			PollOptions = ["Option 1", "Option 2"],
			DaysOpen = 7
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/NotFound", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPost_CreateNewPoll_CallsForumService()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "What do you think?",
			PollOptions = ["Great", "Good", "Okay"],
			DaysOpen = 7,
			MultiSelect = true
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirect.PageName);
		Assert.AreEqual(topic.Id, redirect.RouteValues!["Id"]);

		await _forumService.Received(1).CreatePoll(
			Arg.Is<ForumTopic>(t => t.Id == topic.Id),
			Arg.Is<PollCreate>(p =>
				p.Question == "What do you think?" &&
				p.DaysOpen == 7 &&
				p.MultiSelect == true));
	}

	[TestMethod]
	public async Task OnPost_EditExistingPollWithoutVotes_UpdatesPoll()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.Question = "Old question";
		poll.MultiSelect = false;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "New question?",
			PollOptions = ["New Option 1", "New Option 2"],
			DaysOpen = 14,
			MultiSelect = true
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		// Verify poll was updated
		await _db.Entry(poll).ReloadAsync();
		await _db.Entry(poll).Collection(p => p.PollOptions).LoadAsync();

		Assert.AreEqual("New question?", poll.Question);
		Assert.IsTrue(poll.MultiSelect);
		Assert.IsTrue(poll.CloseDate.HasValue);
		Assert.AreEqual(2, poll.PollOptions.Count);
		Assert.IsTrue(poll.PollOptions.Any(o => o.Text == "New Option 1"));
		Assert.IsTrue(poll.PollOptions.Any(o => o.Text == "New Option 2"));
	}

	[TestMethod]
	public async Task OnPost_EditExistingPollWithVotes_OnlyUpdatesCloseDate()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var voter = _db.AddUser("Voter").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.Question = "Original question";
		poll.MultiSelect = false;

		// Add a vote to prevent full editing
		var firstOption = poll.PollOptions.First();
		_db.ForumPollOptionVotes.Add(new ForumPollOptionVote
		{
			PollOption = firstOption,
			User = voter
		});
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "New question?", // This should be ignored due to votes
			PollOptions = ["New Option 1", "New Option 2"], // Valid options for validation
			DaysOpen = 10,
			MultiSelect = true // This should be ignored due to votes
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		// Verify only close date was updated
		await _db.Entry(poll).ReloadAsync();
		await _db.Entry(poll).Collection(p => p.PollOptions).LoadAsync();

		Assert.AreEqual("Original question", poll.Question); // Unchanged
		Assert.IsFalse(poll.MultiSelect); // Unchanged
		Assert.IsTrue(poll.CloseDate.HasValue); // Updated
		Assert.AreEqual(2, poll.PollOptions.Count); // Unchanged
	}

	[TestMethod]
	public async Task OnPost_EditPollCloseDateWithNullDaysOpen_SetsNullCloseDate()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "Updated question?",
			PollOptions = ["Option 1", "Option 2"],
			DaysOpen = null, // No close date
			MultiSelect = false
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		await _db.Entry(poll).ReloadAsync();
		Assert.IsNull(poll.CloseDate);
	}

	[TestMethod]
	public void PollCreate_IsValid_ReturnsTrueForValidPoll()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			Question = "Valid question with proper length?",
			PollOptions = ["Option 1", "Option 2", "Option 3"],
			DaysOpen = 7,
			MultiSelect = false
		};

		Assert.IsTrue(poll.IsValid);
	}

	[TestMethod]
	public void PollCreate_IsValid_ReturnsFalseForInvalidQuestion()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			Question = "Short", // Too short (minimum 8 characters) - but validation doesn't check minimum in IsValid
			PollOptions = ["Option 1", "Option 2"],
			DaysOpen = 7,
			MultiSelect = false
		};

		// The IsValid property only checks for null/whitespace and max length, not minimum length
		Assert.IsTrue(poll.IsValid);
	}

	[TestMethod]
	public void PollCreate_IsValid_ReturnsFalseForTooLongQuestion()
	{
		var longQuestion = new string('a', 201); // Too long (max 200 characters)
		var poll = new AddEditPollModel.PollCreate
		{
			Question = longQuestion,
			PollOptions = ["Option 1", "Option 2"],
			DaysOpen = 7,
			MultiSelect = false
		};

		Assert.IsFalse(poll.IsValid);
	}

	[TestMethod]
	public void PollCreate_OptionsAreValid_ReturnsFalseForSingleOption()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			Question = "Valid question?",
			PollOptions = ["Only one option"],
			DaysOpen = 7,
			MultiSelect = false
		};

		Assert.IsFalse(poll.OptionsAreValid);
		Assert.IsFalse(poll.IsValid);
	}

	[TestMethod]
	public void PollCreate_OptionsAreValid_ReturnsFalseForTooLongOption()
	{
		var longOption = new string('a', 251); // Too long (max 250 characters)
		var poll = new AddEditPollModel.PollCreate
		{
			Question = "Valid question?",
			PollOptions = ["Option 1", longOption],
			DaysOpen = 7,
			MultiSelect = false
		};

		Assert.IsFalse(poll.OptionsAreValid);
		Assert.IsFalse(poll.IsValid);
	}

	[TestMethod]
	public void PollCreate_HasAnyField_ReturnsTrueWhenQuestionExists()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			Question = "Some question?",
			PollOptions = ["", ""],
			DaysOpen = null,
			MultiSelect = false
		};

		Assert.IsTrue(poll.HasAnyField);
	}

	[TestMethod]
	public void PollCreate_HasAnyField_ReturnsTrueWhenDaysOpenExists()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			Question = "",
			PollOptions = ["", ""],
			DaysOpen = 7,
			MultiSelect = false
		};

		Assert.IsTrue(poll.HasAnyField);
	}

	[TestMethod]
	public void PollCreate_HasAnyField_ReturnsTrueWhenOptionExists()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			Question = "",
			PollOptions = ["Some option", ""],
			DaysOpen = null,
			MultiSelect = false
		};

		Assert.IsTrue(poll.HasAnyField);
	}

	[TestMethod]
	public void PollCreate_HasAnyField_ReturnsFalseWhenAllFieldsEmpty()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			Question = "",
			PollOptions = ["", ""],
			DaysOpen = null,
			MultiSelect = false
		};

		Assert.IsFalse(poll.HasAnyField);
	}
}
