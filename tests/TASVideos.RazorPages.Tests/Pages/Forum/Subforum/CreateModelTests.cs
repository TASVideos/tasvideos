using TASVideos.Pages.Forum.Subforum;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Subforum;

[TestClass]
public class CreateModelTests : BasePageModelTests
{
	private readonly CreateModel _model;

	public CreateModelTests()
	{
		_model = new CreateModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_PopulatesAvailableCategories()
	{
		var category1 = _db.AddForumCategory("Category 1").Entity;
		var category2 = _db.AddForumCategory("Category 2").Entity;
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(2, _model.AvailableCategories.Count);
		Assert.IsTrue(_model.AvailableCategories.Any(c => c.Text == category1.Title));
		Assert.IsTrue(_model.AvailableCategories.Any(c => c.Text == category2.Title));
	}

	[TestMethod]
	public async Task OnGet_EmptyCategories_ReturnsEmptyList()
	{
		await _model.OnGet();
		Assert.AreEqual(0, _model.AvailableCategories.Count);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithCategories()
	{
		_db.AddForumCategory("Test Category");
		await _db.SaveChangesAsync();
		_model.ModelState.AddModelError("Name", "Name is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(1, _model.AvailableCategories.Count);
		Assert.IsFalse(await _db.Forums.AnyAsync());
	}

	[TestMethod]
	public async Task OnPost_ValidModel_CreatesForum()
	{
		var category = _db.AddForumCategory("Test Category").Entity;
		await _db.SaveChangesAsync();
		_model.Forum = new EditModel.ForumEdit
		{
			Name = "New Forum",
			ShortName = "NewForum",
			Description = "Test forum description",
			Category = category.Id,
			RestrictedAccess = false
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");

		var createdForum = await _db.Forums.SingleOrDefaultAsync(f => f.Name == "New Forum");
		Assert.IsNotNull(createdForum);
		Assert.AreEqual("New Forum", createdForum.Name);
		Assert.AreEqual("NewForum", createdForum.ShortName);
		Assert.AreEqual("Test forum description", createdForum.Description);
		Assert.AreEqual(category.Id, createdForum.CategoryId);
		Assert.IsFalse(createdForum.Restricted);
	}

	[TestMethod]
	public async Task OnPost_RestrictedForum_CreatesRestrictedForum()
	{
		var category = _db.AddForumCategory("Test Category").Entity;
		await _db.SaveChangesAsync();
		_model.Forum = new EditModel.ForumEdit
		{
			Name = "Restricted Forum",
			Category = category.Id,
			RestrictedAccess = true
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		var createdForum = await _db.Forums.SingleOrDefaultAsync(f => f.Name == "Restricted Forum");
		Assert.IsNotNull(createdForum);
		Assert.IsTrue(createdForum.Restricted);
	}

	[TestMethod]
	public async Task OnPost_NonExistentCategory_ReturnsNotFound()
	{
		_model.Forum = new EditModel.ForumEdit { Category = 999 };
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_HandlesGracefully()
	{
		var category = _db.AddForumCategory("Test Category").Entity;
		await _db.SaveChangesAsync();
		_model.Forum = new EditModel.ForumEdit { Category = category.Id };
		_db.CreateUpdateConflict();

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
	}

	[TestMethod]
	public async Task OnPost_WithValidInput_RedirectsToIndexWithForumId()
	{
		var category = _db.AddForumCategory("Test Category").Entity;
		await _db.SaveChangesAsync();
		_model.Forum = new EditModel.ForumEdit
		{
			Name = "Test Forum",
			Category = category.Id
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		var redirectResult = (RedirectToPageResult)result;
		Assert.IsNotNull(redirectResult.RouteValues);
	}

	[TestMethod]
	public async Task OnPost_MultipleForums_CreatesAllSuccessfully()
	{
		var category = _db.AddForumCategory("Test Category").Entity;
		await _db.SaveChangesAsync();

		_model.Forum = new EditModel.ForumEdit
		{
			Name = "Forum 1",
			Category = category.Id,
			RestrictedAccess = false
		};
		await _model.OnPost();

		_model.Forum = new EditModel.ForumEdit
		{
			Name = "Forum 2",
			Category = category.Id,
			RestrictedAccess = true
		};
		await _model.OnPost();

		var forums = await _db.Forums.ToListAsync();
		Assert.AreEqual(2, forums.Count);
		Assert.IsTrue(forums.Any(f => f is { Name: "Forum 1", Restricted: false }));
		Assert.IsTrue(forums.Any(f => f is { Name: "Forum 2", Restricted: true }));
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(CreateModel), PermissionTo.EditForums);
}
