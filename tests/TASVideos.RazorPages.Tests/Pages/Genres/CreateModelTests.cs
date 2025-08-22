using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages;
using TASVideos.Pages.Genres;

namespace TASVideos.RazorPages.Tests.Pages.Genres;

[TestClass]
public class CreateModelTests : BasePageModelTests
{
	private readonly IGenreService _genreService;
	private readonly CreateModel _model;

	public CreateModelTests()
	{
		_genreService = Substitute.For<IGenreService>();
		_model = new CreateModel(_genreService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("Genre.DisplayName", "Name is required");
		_model.Genre = new Genre { DisplayName = "" };

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _genreService.DidNotReceive().Add(Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_ValidGenre_SuccessfulCreation_RedirectsToIndex()
	{
		_genreService.Add("Action").Returns(1);
		_model.Genre = new Genre { DisplayName = "Action" };

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirectResult.PageName);
		await _genreService.Received(1).Add("Action");
	}

	[TestMethod]
	public async Task OnPost_ValidGenre_CreationReturnsNull_ShowsErrorAndReturnsPage()
	{
		_genreService.Add("Failed Genre").Returns((int?)null);
		_model.Genre = new Genre { DisplayName = "Failed Genre" };

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _genreService.Received(1).Add("Failed Genre");
	}

	[TestMethod]
	public void CreateModel_HasRequirePermissionAttribute()
	{
		var type = typeof(CreateModel);
		var attribute = type.GetCustomAttributes(typeof(RequirePermissionAttribute), false).FirstOrDefault() as RequirePermissionAttribute;

		Assert.IsNotNull(attribute);
		Assert.IsTrue(attribute.RequiredPermissions.Contains(PermissionTo.TagMaintenance));
	}
}
