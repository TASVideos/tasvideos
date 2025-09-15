using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class SetTypeModelTests : BasePageModelTests
{
	private readonly SetTypeModel _model;

	public SetTypeModelTests()
	{
		_model = new SetTypeModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_InvalidTopicId_ReturnsNotFound()
	{
		_model.TopicId = 999;
		var result = await _model.OnGet();
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopic_WithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_ExistingTopic_PopulatesProperties()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		topic.Type = ForumTopicType.Sticky;
		await _db.SaveChangesAsync();

		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Topic", _model.TopicTitle);
		Assert.AreEqual(ForumTopicType.Sticky, _model.Type);
		Assert.AreEqual(topic.ForumId, _model.ForumId);
		Assert.AreEqual(topic.Forum!.Name, _model.ForumName);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopic_WithPermission_ReturnsPage()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType, PermissionTo.SeeRestrictedForums]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("Type", "Invalid type");
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_ValidUpdate_ChangesTopicType()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Type = ForumTopicType.Regular;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType]);
		_model.TopicId = topic.Id;
		_model.Type = ForumTopicType.Sticky;

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual(topic.Id, redirect.RouteValues!["Id"]);

		await _db.Entry(topic).ReloadAsync();
		Assert.AreEqual(ForumTopicType.Sticky, topic.Type);
	}

	[TestMethod]
	public async Task OnPost_RestrictedTopic_WithoutPermission_ReturnsNotFound()
	{
		var topic = _db.AddTopic(null, true).Entity;
		await _db.SaveChangesAsync();
		_model.TopicId = topic.Id;

		var result = await _model.OnPost();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPost_RestrictedTopic_WithPermission_UpdatesSuccessfully()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType, PermissionTo.SeeRestrictedForums]);
		_model.TopicId = topic.Id;

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_HandlesGracefully()
	{
		var topic = _db.AddTopic().Entity;
		topic.Type = ForumTopicType.Regular;
		await _db.SaveChangesAsync();

		_db.CreateUpdateConflict();
		_model.TopicId = topic.Id;
		_model.Type = ForumTopicType.Sticky;

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
	}
}
