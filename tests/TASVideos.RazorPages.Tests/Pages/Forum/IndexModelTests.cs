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
	public async Task OnGet_EmptyCategories_SetsEmptyCollection()
	{
		_forumService.GetAllCategories().Returns([]);
		await _model.OnGet();
		Assert.AreEqual(0, _model.Categories.Count);
	}

	[TestMethod]
	public async Task OnGet_MultipleCategoriesWithForums_PopulatesCorrectly()
	{
		_forumService.GetAllCategories().Returns(
		[
			new()
			{
				Id = 1,
				Ordinal = 2,
				Title = "General",
				Description = "General discussion",
				Forums =
				[
					new()
					{
						Id = 1,
						Ordinal = 1,
						Name = "General Discussion",
						Description = "Talk about anything"
					},
					new()
					{
						Id = 2,
						Ordinal = 2,
						Name = "Newcomers",
						Description = "New user introductions"
					}
				]
			},
			new()
			{
				Id = 2,
				Ordinal = 1,
				Title = "TAS Discussion",
				Description = "Tool-assisted speedrun topics",
				Forums =
				[
					new()
					{
						Id = 3,
						Ordinal = 1,
						Name = "TAS Help",
						Description = "Get help with your TAS"
					}
				]
			}
		]);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Categories.Count);

		var generalCategory = _model.Categories.SingleOrDefault(c => c.Title == "General");
		Assert.IsNotNull(generalCategory);
		Assert.AreEqual(2, generalCategory.Forums.Count());
		Assert.IsTrue(generalCategory.Forums.Any(f => f.Name == "General Discussion"));
		Assert.IsTrue(generalCategory.Forums.Any(f => f.Name == "Newcomers"));

		var tasCategory = _model.Categories.SingleOrDefault(c => c.Title == "TAS Discussion");
		Assert.IsNotNull(tasCategory);
		Assert.AreEqual(1, tasCategory.Forums.Count());
		Assert.IsTrue(tasCategory.Forums.Any(f => f.Name == "TAS Help"));
	}
}
