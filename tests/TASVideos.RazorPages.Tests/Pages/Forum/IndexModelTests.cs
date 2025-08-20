using TASVideos.Core.Services;
using TASVideos.Pages.Forum;

namespace TASVideos.RazorPages.Tests.Pages.Forum;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IForumService _forumService;
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_forumService = Substitute.For<IForumService>();
		_model = new IndexModel(_forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_CallsForumService()
	{
		_forumService.GetAllCategories().Returns(
		[
			new()
			{
				Id = 1,
				Ordinal = 1,
				Title = "General",
				Description = "General discussion",
				Forums =
				[
					new()
					{
						Id = 1,
						Ordinal = 1,
						Name = "General Discussion",
						Description = "Talk about anything",
						Restricted = false
					}
				]
			}
		]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Categories.Count);
		Assert.AreEqual("General", _model.Categories.First().Title);
		await _forumService.Received(1).GetAllCategories();
	}

	[TestMethod]
	public async Task OnGet_EmptyCategories_SetsEmptyCollection()
	{
		_forumService.GetAllCategories().Returns([]);

		await _model.OnGet();

		Assert.AreEqual(0, _model.Categories.Count);
		await _forumService.Received(1).GetAllCategories();
	}

	[TestMethod]
	public async Task OnGet_MultipleCategoriesWithForums_PopulatesCorrectly()
	{
		_forumService.GetAllCategories().Returns(
		[
			new()
			{
				Id = 1,
				Ordinal = 1,
				Title = "General",
				Description = "General discussion",
				Forums =
				[
					new()
					{
						Id = 1,
						Ordinal = 1,
						Name = "General Discussion",
						Description = "Talk about anything",
						Restricted = false
					},
					new()
					{
						Id = 2,
						Ordinal = 2,
						Name = "Newcomers",
						Description = "New user introductions",
						Restricted = false
					}
				]
			},
			new()
			{
				Id = 2,
				Ordinal = 2,
				Title = "TAS Discussion",
				Description = "Tool-assisted speedrun topics",
				Forums =
				[
					new()
					{
						Id = 3,
						Ordinal = 1,
						Name = "TAS Help",
						Description = "Get help with your TAS",
						Restricted = false
					}
				]
			}
		]);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Categories.Count);

		var generalCategory = _model.Categories.First(c => c.Title == "General");
		Assert.AreEqual(2, generalCategory.Forums.Count());
		Assert.IsTrue(generalCategory.Forums.Any(f => f.Name == "General Discussion"));
		Assert.IsTrue(generalCategory.Forums.Any(f => f.Name == "Newcomers"));

		var tasCategory = _model.Categories.First(c => c.Title == "TAS Discussion");
		Assert.AreEqual(1, tasCategory.Forums.Count());
		Assert.IsTrue(tasCategory.Forums.Any(f => f.Name == "TAS Help"));
	}

	[TestMethod]
	public async Task OnGet_CategoriesWithRestrictedForums_IncludesRestrictedForums()
	{
		_forumService.GetAllCategories().Returns(
		[
			new()
			{
				Id = 1,
				Ordinal = 1,
				Title = "Administration",
				Description = "Admin discussions",
				Forums =
				[
					new()
					{
						Id = 1,
						Ordinal = 1,
						Name = "Staff Forum",
						Description = "Staff only discussions",
						Restricted = true
					},
					new()
					{
						Id = 2,
						Ordinal = 2,
						Name = "Public Announcements",
						Description = "Public announcements",
						Restricted = false
					}
				]
			}
		]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Categories.Count);
		var category = _model.Categories.First();
		Assert.AreEqual(2, category.Forums.Count());

		var restrictedForum = category.Forums.First(f => f.Name == "Staff Forum");
		Assert.IsTrue(restrictedForum.Restricted);

		var publicForum = category.Forums.First(f => f.Name == "Public Announcements");
		Assert.IsFalse(publicForum.Restricted);
	}

	[TestMethod]
	public async Task OnGet_CategoriesWithNoForums_HandlesEmptyForumCollections()
	{
		_forumService.GetAllCategories().Returns(
		[
			new()
			{
				Id = 1,
				Ordinal = 1,
				Title = "Empty Category",
				Description = "A category with no forums",
				Forums = []
			}
		]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Categories.Count);
		var category = _model.Categories.First();
		Assert.AreEqual("Empty Category", category.Title);
		Assert.AreEqual(0, category.Forums.Count());
	}

	[TestMethod]
	public async Task OnGet_CategoriesOrderedByOrdinal_MaintainsOrder()
	{
		_forumService.GetAllCategories().Returns(
		[
			new()
			{
				Id = 2,
				Ordinal = 2,
				Title = "Second Category",
				Description = "Second in order",
				Forums = []
			},
			new()
			{
				Id = 1,
				Ordinal = 1,
				Title = "First Category",
				Description = "First in order",
				Forums = []
			},
			new()
			{
				Id = 3,
				Ordinal = 3,
				Title = "Third Category",
				Description = "Third in order",
				Forums = []
			}
		]);

		await _model.OnGet();

		Assert.AreEqual(3, _model.Categories.Count);
		var categoryList = _model.Categories.ToList();
		Assert.AreEqual("Second Category", categoryList[0].Title);
		Assert.AreEqual("First Category", categoryList[1].Title);
		Assert.AreEqual("Third Category", categoryList[2].Title);
	}
}
