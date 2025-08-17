using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

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

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/NotFound", redirectResult.PageName);
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
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		topic.Title = "Restricted Topic";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType, PermissionTo.SeeRestrictedForums]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Restricted Topic", _model.TopicTitle);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopic_WithoutPermission_ReturnsPage()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		topic.Title = "Restricted Topic";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/NotFound", redirectResult.PageName);
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

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirect.PageName);
		Assert.AreEqual(topic.Id, redirect.RouteValues!["Id"]);

		await _db.Entry(topic).ReloadAsync();
		Assert.AreEqual(ForumTopicType.Sticky, topic.Type);
	}

	[TestMethod]
	public async Task OnPost_RestrictedTopic_WithPermission_UpdatesSuccessfully()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		topic.Type = ForumTopicType.Regular;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType, PermissionTo.SeeRestrictedForums]);
		_model.TopicId = topic.Id;
		_model.Type = ForumTopicType.Announcement;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		await _db.Entry(topic).ReloadAsync();
		Assert.AreEqual(ForumTopicType.Announcement, topic.Type);
	}

	[TestMethod]
	public async Task OnPost_RestrictedTopic_WithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		topic.Type = ForumTopicType.Regular;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType]);
		_model.TopicId = topic.Id;
		_model.Type = ForumTopicType.Announcement;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/NotFound", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPost_SameType_StillUpdatesSuccessfully()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Type = ForumTopicType.Sticky;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType]);
		_model.TopicId = topic.Id;
		_model.Type = ForumTopicType.Sticky;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		await _db.Entry(topic).ReloadAsync();
		Assert.AreEqual(ForumTopicType.Sticky, topic.Type);
	}

	[TestMethod]
	public void ForumTopicType_EnumValues_AreCorrect()
	{
		// These assertions verify the enum values match expected database values
		var regularValue = (int)ForumTopicType.Regular;
		var stickyValue = (int)ForumTopicType.Sticky;
		var announcementValue = (int)ForumTopicType.Announcement;

		Assert.AreEqual(0, regularValue);
		Assert.AreEqual(1, stickyValue);
		Assert.AreEqual(2, announcementValue);
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_HandlesGracefully()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Type = ForumTopicType.Regular;
		await _db.SaveChangesAsync();

		_db.CreateUpdateConflict();
		AddAuthenticatedUser(_model, user, [PermissionTo.SetTopicType]);
		_model.TopicId = topic.Id;
		_model.Type = ForumTopicType.Sticky;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
	}
}
