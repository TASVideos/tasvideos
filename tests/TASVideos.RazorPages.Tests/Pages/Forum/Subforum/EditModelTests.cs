using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;
using TASVideos.Pages;
using TASVideos.Pages.Forum.Subforum;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Subforum;

[TestClass]
public class EditModelTests : BasePageModelTests
{
	private readonly EditModel _model;

	public EditModelTests()
	{
		_model = new EditModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentForum_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_RestrictedForumWithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("RegularUser").Entity;
		var forum = _db.AddForum("Restricted Forum", true).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);
		_model.Id = forum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ExistingForum_LoadsForumData()
	{
		var forum = _db.AddForum("Test Forum").Entity;
		forum.Description = "Test description";
		forum.ShortName = "TestForum";
		forum.Restricted = false;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Forum", _model.Forum.Name);
		Assert.AreEqual("TestForum", _model.Forum.ShortName);
		Assert.AreEqual("Test description", _model.Forum.Description);
		Assert.AreEqual(forum.CategoryId, _model.Forum.Category);
		Assert.IsFalse(_model.Forum.RestrictedAccess);
		Assert.AreEqual(1, _model.AvailableCategories.Count);
	}

	[TestMethod]
	public async Task OnGet_ForumWithoutTopics_CanDelete()
	{
		var forum = _db.AddForum("Empty Forum").Entity;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;

		await _model.OnGet();

		Assert.IsTrue(_model.CanDelete);
	}

	[TestMethod]
	public async Task OnGet_ForumWithTopics_CannotDelete()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var forum = _db.AddForum("Forum With Topics").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = forum;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;

		await _model.OnGet();

		Assert.IsFalse(_model.CanDelete);
	}

	[TestMethod]
	public async Task OnPost_NonExistentForum_ReturnsNotFound()
	{
		_model.Id = 999;
		_model.Forum = new EditModel.ForumEdit
		{
			Name = "Updated Forum",
			ShortName = "Updated",
			Category = 1,
			RestrictedAccess = false
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_RestrictedForumWithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("RegularUser").Entity;
		var forum = _db.AddForum("Restricted Forum", true).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);
		_model.Id = forum.Id;
		_model.Forum = new EditModel.ForumEdit
		{
			Name = "Updated Forum",
			ShortName = "Updated",
			Category = 1,
			RestrictedAccess = false
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithCategories()
	{
		var category = _db.AddForumCategory("Test Category").Entity;
		var forum = _db.AddForum("Test Forum").Entity;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;
		_model.Forum = new EditModel.ForumEdit
		{
			Category = category.Id
		};
		_model.ModelState.AddModelError("Name", "Name is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(_db.ForumCategories.Count(), _model.AvailableCategories.Count);
	}

	[TestMethod]
	public async Task OnPost_ValidData_UpdatesForum()
	{
		var category1 = _db.AddForumCategory("Original Category").Entity;
		var category2 = _db.AddForumCategory("New Category").Entity;
		var forum = _db.AddForum("Original Forum").Entity;
		forum.CategoryId = category1.Id;
		forum.Description = "Original description";
		forum.ShortName = "OrigForum";
		forum.Restricted = false;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;
		_model.Forum = new EditModel.ForumEdit
		{
			Name = "Updated Forum",
			ShortName = "UpdatedForum",
			Description = "Updated description",
			Category = category2.Id,
			RestrictedAccess = true
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirectResult.PageName);
		Assert.AreEqual(forum.Id, redirectResult.RouteValues!["Id"]);

		var updatedForum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == forum.Id);
		Assert.IsNotNull(updatedForum);
		Assert.AreEqual("Updated Forum", updatedForum.Name);
		Assert.AreEqual("UpdatedForum", updatedForum.ShortName);
		Assert.AreEqual("Updated description", updatedForum.Description);
		Assert.AreEqual(category2.Id, updatedForum.CategoryId);
		Assert.IsTrue(updatedForum.Restricted);
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_HandlesGracefully()
	{
		var category = _db.AddForumCategory("Test Category").Entity;
		var forum = _db.AddForum("Test Forum").Entity;
		forum.CategoryId = category.Id;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;
		_model.Forum = new EditModel.ForumEdit
		{
			Name = "Updated Forum",
			ShortName = "Updated",
			Category = category.Id
		};

		_db.CreateUpdateConflict();

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
	}

	[TestMethod]
	public async Task OnPostDelete_NonExistentForum_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPostDelete_ForumWithTopics_ReturnsBadRequest()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var forum = _db.AddForum("Forum With Topics").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = forum;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
		var badRequestResult = (BadRequestObjectResult)result;
		Assert.AreEqual("Cannot delete subforum that contains topics", badRequestResult.Value);
	}

	[TestMethod]
	public async Task OnPostDelete_EmptyForum_DeletesSuccessfully()
	{
		var forum = _db.AddForum("Empty Forum").Entity;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/Index", redirectResult.PageName);

		var deletedForum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == forum.Id);
		Assert.IsNull(deletedForum);
	}

	[TestMethod]
	public async Task OnPostDelete_DatabaseSaveFailure_HandlesGracefully()
	{
		var forum = _db.AddForum("Test Forum").Entity;
		await _db.SaveChangesAsync();

		_model.Id = forum.Id;
		_db.CreateUpdateConflict();

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
	}

	[TestMethod]
	public void EditModel_HasRequirePermissionAttribute()
	{
		var type = typeof(EditModel);
		var attribute = type.GetCustomAttributes(typeof(RequirePermissionAttribute), false).FirstOrDefault() as RequirePermissionAttribute;

		Assert.IsNotNull(attribute);
		Assert.IsTrue(attribute.RequiredPermissions.Contains(PermissionTo.EditForums));
	}
}
