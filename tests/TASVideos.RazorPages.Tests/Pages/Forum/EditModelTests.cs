using TASVideos.Pages.Forum;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Forum;

[TestClass]
public class EditModelTests : TestDbBase
{
	private readonly EditModel _model;

	public EditModelTests()
	{
		_model = new EditModel(_db);
	}

	[TestMethod]
	public async Task OnGet_InvalidId_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ValidId_ReturnsPageAndPopulatesCategory()
	{
		var category = _db.AddForumCategory("Test Category").Entity;
		category.Description = "Test Description";

		var forum1 = _db.AddForum("Forum 1").Entity;
		forum1.Ordinal = 2;
		forum1.Category = category;

		var forum2 = _db.AddForum("Forum 2").Entity;
		forum2.Ordinal = 1;
		forum2.Category = category;

		await _db.SaveChangesAsync();

		_model.Id = category.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Category", _model.Category.Title);
		Assert.AreEqual("Test Description", _model.Category.Description);
		Assert.AreEqual(2, _model.Category.Forums.Count);

		var orderedForums = _model.Category.Forums.OrderBy(f => f.Ordinal).ToList();
		Assert.AreEqual("Forum 2", orderedForums[0].Name);
		Assert.AreEqual(1, orderedForums[0].Ordinal);
		Assert.AreEqual("Forum 1", orderedForums[1].Name);
		Assert.AreEqual(2, orderedForums[1].Ordinal);
	}

	[TestMethod]
	public async Task OnGet_CategoryWithNoForums_ReturnsPageWithEmptyForumsList()
	{
		var category = _db.AddForumCategory("Empty Category").Entity;
		await _db.SaveChangesAsync();
		_model.Id = category.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Empty Category", _model.Category.Title);
		Assert.AreEqual(0, _model.Category.Forums.Count);
	}

	[TestMethod]
	public async Task OnPost_InvalidId_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("Title", "Title is required");
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_ValidModel_UpdatesCategoryAndForumsAndReorders()
	{
		var category = _db.AddForumCategory("Original Title").Entity;
		category.Description = "Original Description";

		var forum1 = _db.AddForum("Forum 1").Entity;
		forum1.Description = "Forum 1 Description";
		forum1.Ordinal = 2;
		forum1.Category = category;

		var forum2 = _db.AddForum("Forum 2").Entity;
		forum2.Description = "Forum 2 Description";
		forum2.Ordinal = 1;
		forum2.Category = category;
		await _db.SaveChangesAsync();

		_model.Id = category.Id;
		_model.Category = new EditModel.CategoryEdit
		{
			Title = "Updated Title",
			Description = "Updated Description",
			Forums =
			[
				new EditModel.CategoryEdit.ForumEdit
				{
					Id = forum1.Id,
					Name = "Forum 1",
					Description = "Forum 1 Description",
					Ordinal = 1
				},
				new EditModel.CategoryEdit.ForumEdit
				{
					Id = forum2.Id,
					Name = "Forum 2",
					Description = "Forum 2 Description",
					Ordinal = 2
				}
			]
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");

		var updatedCategory = await _db.ForumCategories
			.Include(c => c.Forums)
			.SingleOrDefaultAsync(c => c.Id == category.Id);

		Assert.IsNotNull(updatedCategory);
		Assert.AreEqual("Updated Title", updatedCategory.Title);
		Assert.AreEqual("Updated Description", updatedCategory.Description);

		var updatedForum1 = updatedCategory.Forums.Single(f => f.Id == forum1.Id);
		var updatedForum2 = updatedCategory.Forums.Single(f => f.Id == forum2.Id);
		Assert.AreEqual(1, updatedForum1.Ordinal);
		Assert.AreEqual(2, updatedForum2.Ordinal);
	}
}
