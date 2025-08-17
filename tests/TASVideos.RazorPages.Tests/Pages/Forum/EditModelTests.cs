using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity.Forum;
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
		var category = _db.ForumCategories.Add(new ForumCategory
		{
			Title = "Test Category",
			Description = "Test Description",
			Ordinal = 1
		}).Entity;

		_db.Forums.AddRange(
			new TASVideos.Data.Entity.Forum.Forum
			{
				Name = "Forum 1",
				Description = "Forum 1 Description",
				Ordinal = 2,
				Category = category
			},
			new TASVideos.Data.Entity.Forum.Forum
			{
				Name = "Forum 2",
				Description = "Forum 2 Description",
				Ordinal = 1,
				Category = category
			});
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
		var category = _db.ForumCategories.Add(new ForumCategory
		{
			Title = "Empty Category",
			Description = "Category with no forums",
			Ordinal = 1
		}).Entity;
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
		_model.Category = new EditModel.CategoryEdit
		{
			Title = "Test Title",
			Description = "Test Description",
			Forums = []
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_ValidModel_UpdatesCategoryAndForums()
	{
		var category = _db.ForumCategories.Add(new ForumCategory
		{
			Title = "Original Title",
			Description = "Original Description",
			Ordinal = 1
		}).Entity;

		var forum1 = new TASVideos.Data.Entity.Forum.Forum
		{
			Name = "Forum 1",
			Description = "Forum 1 Description",
			Ordinal = 2,
			Category = category
		};

		var forum2 = new TASVideos.Data.Entity.Forum.Forum
		{
			Name = "Forum 2",
			Description = "Forum 2 Description",
			Ordinal = 1,
			Category = category
		};

		_db.Forums.AddRange(forum1, forum2);
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

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirectResult.PageName);

		var updatedCategory = await _db.ForumCategories
			.Include(c => c.Forums)
			.SingleAsync(c => c.Id == category.Id);

		Assert.AreEqual("Updated Title", updatedCategory.Title);
		Assert.AreEqual("Updated Description", updatedCategory.Description);

		var updatedForum1 = updatedCategory.Forums.Single(f => f.Id == forum1.Id);
		var updatedForum2 = updatedCategory.Forums.Single(f => f.Id == forum2.Id);
		Assert.AreEqual(1, updatedForum1.Ordinal);
		Assert.AreEqual(2, updatedForum2.Ordinal);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		var category = _db.ForumCategories.Add(new ForumCategory
		{
			Title = "Test Category",
			Description = "Test Description",
			Ordinal = 1
		}).Entity;
		await _db.SaveChangesAsync();

		_model.Id = category.Id;
		_model.ModelState.AddModelError("Title", "Title is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_ReorderForums_UpdatesOrdinals()
	{
		var category = _db.ForumCategories.Add(new ForumCategory
		{
			Title = "Test Category",
			Description = "Test Description",
			Ordinal = 1
		}).Entity;

		var forum1 = new TASVideos.Data.Entity.Forum.Forum
		{
			Name = "Forum 1",
			Description = "Forum 1 Description",
			Ordinal = 1,
			Category = category
		};

		var forum2 = new TASVideos.Data.Entity.Forum.Forum
		{
			Name = "Forum 2",
			Description = "Forum 2 Description",
			Ordinal = 2,
			Category = category
		};

		var forum3 = new TASVideos.Data.Entity.Forum.Forum
		{
			Name = "Forum 3",
			Description = "Forum 3 Description",
			Ordinal = 3,
			Category = category
		};

		_db.Forums.AddRange(forum1, forum2, forum3);
		await _db.SaveChangesAsync();

		_model.Id = category.Id;
		_model.Category = new EditModel.CategoryEdit
		{
			Title = "Test Category",
			Description = "Test Description",
			Forums =
			[
				new EditModel.CategoryEdit.ForumEdit
				{
					Id = forum3.Id,
					Name = "Forum 3",
					Description = "Forum 3 Description",
					Ordinal = 1
				},
				new EditModel.CategoryEdit.ForumEdit
				{
					Id = forum1.Id,
					Name = "Forum 1",
					Description = "Forum 1 Description",
					Ordinal = 2
				},
				new EditModel.CategoryEdit.ForumEdit
				{
					Id = forum2.Id,
					Name = "Forum 2",
					Description = "Forum 2 Description",
					Ordinal = 3
				}
			]
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		var updatedCategory = await _db.ForumCategories
			.Include(c => c.Forums)
			.SingleAsync(c => c.Id == category.Id);

		var updatedForum1 = updatedCategory.Forums.Single(f => f.Id == forum1.Id);
		var updatedForum2 = updatedCategory.Forums.Single(f => f.Id == forum2.Id);
		var updatedForum3 = updatedCategory.Forums.Single(f => f.Id == forum3.Id);

		Assert.AreEqual(2, updatedForum1.Ordinal);
		Assert.AreEqual(3, updatedForum2.Ordinal);
		Assert.AreEqual(1, updatedForum3.Ordinal);
	}
}
