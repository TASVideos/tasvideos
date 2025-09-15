using TASVideos.Core.Services;
using TASVideos.Pages.Genres;

namespace TASVideos.RazorPages.Tests.Pages.Genres;

[TestClass]
public class EditModelTests : BasePageModelTests
{
	private readonly IGenreService _genreService;
	private readonly EditModel _model;

	public EditModelTests()
	{
		_genreService = Substitute.For<IGenreService>();
		_model = new EditModel(_genreService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_GenreNotFound_ReturnsNotFound()
	{
		_genreService.GetById(Arg.Any<int>()).Returns((GenreDto?)null);
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ValidGenre_LoadsGenreData()
	{
		var genreDto = new GenreDto(1, "Action", 5);
		_genreService.GetById(1).Returns(genreDto);
		_genreService.InUse(1).Returns(true);
		_model.Id = 1;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(genreDto.Id, _model.Genre.Id);
		Assert.AreEqual(genreDto.DisplayName, _model.Genre.DisplayName);
		Assert.AreEqual(genreDto.GameCount, _model.Genre.GameCount);
		Assert.IsTrue(_model.InUse);
	}

	[TestMethod]
	public async Task OnGet_GenreNotInUse_SetsInUseFalse()
	{
		var genreDto = new GenreDto(2, "Adventure", 0);
		_genreService.GetById(2).Returns(genreDto);
		_genreService.InUse(2).Returns(false);
		_model.Id = 2;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Adventure", _model.Genre.DisplayName);
		Assert.IsFalse(_model.InUse);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("Genre.DisplayName", "Name is required");
		_model.Genre = new GenreDto(1, "", 0);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _genreService.DidNotReceive().Edit(Arg.Any<int>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_ValidInput_SuccessfulUpdate_RedirectsToIndex()
	{
		_genreService.Edit(1, "Updated Action").Returns(GenreChangeResult.Success);
		_model.Id = 1;
		_model.Genre = new GenreDto(1, "Updated Action", 0);

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		await _genreService.Received(1).Edit(1, "Updated Action");
	}

	[TestMethod]
	public async Task OnPost_GenreNotFound_ReturnsNotFound()
	{
		_genreService.Edit(999, "Non-existent").Returns(GenreChangeResult.NotFound);
		_model.Id = 999;
		_model.Genre = new GenreDto(999, "Non-existent", 0);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		await _genreService.Received(1).Edit(999, "Non-existent");
	}

	[TestMethod]
	public async Task OnPost_EditFails_ShowsErrorAndReturnsPage()
	{
		_genreService.Edit(1, "Failed Genre").Returns(GenreChangeResult.Fail);
		_model.Id = 1;
		_model.Genre = new GenreDto(1, "Failed Genre", 0);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _genreService.Received(1).Edit(1, "Failed Genre");
	}

	[TestMethod]
	public async Task OnPostDelete_Success_ShowsSuccessMessageAndRedirectsToIndex()
	{
		_genreService.Delete(1).Returns(GenreChangeResult.Success);
		_model.Id = 1;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Index");
		await _genreService.Received(1).Delete(1);
	}

	[TestMethod]
	public async Task OnPostDelete_GenreInUse_ShowsErrorAndRedirectsToIndex()
	{
		_genreService.Delete(1).Returns(GenreChangeResult.InUse);
		_model.Id = 1;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Index");
		await _genreService.Received(1).Delete(1);
	}

	[TestMethod]
	public async Task OnPostDelete_GenreNotFound_ShowsErrorAndRedirectsToIndex()
	{
		_genreService.Delete(999).Returns(GenreChangeResult.NotFound);
		_model.Id = 999;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Index");
		await _genreService.Received(1).Delete(999);
	}

	[TestMethod]
	public async Task OnPostDelete_DeleteFails_ShowsErrorAndRedirectsToIndex()
	{
		_genreService.Delete(1).Returns(GenreChangeResult.Fail);
		_model.Id = 1;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Index");
		await _genreService.Received(1).Delete(1);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(CreateModel), PermissionTo.TagMaintenance);
}
