using TASVideos.Core.Services;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics;

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
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_TopicWithoutPoll_PopulatesTopicTitle()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		await _db.SaveChangesAsync();
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
		var voter = _db.AddUser("Test User").Entity;
		var topic = _db.AddTopic().Entity;
		topic.Title = "Topic with Poll";
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.MultiSelect = true;
		_db.VoteForOption(poll.PollOptions.First(), voter);
		await _db.SaveChangesAsync();
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Topic with Poll", _model.TopicTitle);
		Assert.AreEqual(poll.Id, _model.PollId);
		Assert.AreEqual(poll.Question, _model.Poll.Question);
		Assert.IsTrue(_model.Poll.MultiSelect);
		Assert.IsTrue(_model.Poll.DaysOpen > 0);
		Assert.AreEqual(2, _model.Poll.PollOptions.Count);
		Assert.IsTrue(_model.Poll.HasVotes);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopicWithoutPermission_ReturnsNotFound()
	{
		var topic = _db.AddTopic(null, true).Entity;
		await _db.SaveChangesAsync();
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopicWithPermission_ReturnsPage()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
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
		_model.Poll = ValidPoll;

		var result = await _model.OnPost();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPost_RestrictedTopicWithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;
		_model.Poll = ValidPoll;

		var result = await _model.OnPost();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPost_CreateNewPoll_Success()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPolls]);
		_model.TopicId = topic.Id;
		_model.Poll = ValidPoll;

		var result = await _model.OnPost();

		AssertRedirect(result, "Index", topic.Id);
		await _forumService.Received(1).CreatePoll(
			Arg.Is<ForumTopic>(t => t.Id == topic.Id),
			Arg.Is<PollCreate>(p =>
				p.Question == _model.Poll.Question &&
				p.DaysOpen == _model.Poll.DaysOpen));
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

		AssertRedirect(result, "Index");

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
		var topic = _db.AddTopic(user).Entity;
		var poll = _db.CreatePollForTopic(topic).Entity;
		poll.Question = "Original question";
		poll.MultiSelect = false;

		_db.VoteForOption(poll.PollOptions.First(), user);
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

		AssertRedirect(result, "Index");

		// Verify only close date was updated
		await _db.Entry(poll).ReloadAsync();
		await _db.Entry(poll).Collection(p => p.PollOptions).LoadAsync();

		Assert.AreEqual("Original question", poll.Question); // Unchanged
		Assert.IsFalse(poll.MultiSelect); // Unchanged
		Assert.IsTrue(poll.CloseDate.HasValue); // Updated
		Assert.AreEqual(2, poll.PollOptions.Count); // Unchanged
	}

	[TestMethod]
	public void PollCreate_IsValid_ReturnsTrueForValidPoll()
		=> Assert.IsTrue(ValidPoll.IsValid);

	[TestMethod]
	public void PollCreate_IsValid_ReturnsFalseForTooLongQuestion()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			Question = new string('a', 201), // Too long (max 200 characters),
			PollOptions = ["Option 1", "Option 2"],
			DaysOpen = 7
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
			DaysOpen = 7
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
			DaysOpen = 7
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
			DaysOpen = null
		};

		Assert.IsTrue(poll.HasAnyField);
	}

	[TestMethod]
	public void PollCreate_HasAnyField_ReturnsTrueWhenDaysOpenExists()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			PollOptions = ["", ""],
			DaysOpen = 7
		};

		Assert.IsTrue(poll.HasAnyField);
	}

	[TestMethod]
	public void PollCreate_HasAnyField_ReturnsTrueWhenOptionExists()
	{
		var poll = new AddEditPollModel.PollCreate
		{
			PollOptions = ["Some option", ""],
			DaysOpen = null
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

	private static AddEditPollModel.PollCreate ValidPoll => new()
	{
		Question = "Valid question?",
		PollOptions = ["Option 1", "Option 2"],
		DaysOpen = 7
	};
}
