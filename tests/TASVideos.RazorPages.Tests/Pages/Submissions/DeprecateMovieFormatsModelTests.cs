using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Submissions;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class DeprecateMovieFormatsModelTests : TestDbBase
{
	private readonly IMovieFormatDeprecator _deprecator;
	private readonly IExternalMediaPublisher _publisher;
	private readonly DeprecateMovieFormatsModel _page;

	public DeprecateMovieFormatsModelTests()
	{
		_deprecator = Substitute.For<IMovieFormatDeprecator>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_page = new DeprecateMovieFormatsModel(_deprecator, _publisher);
	}

	[TestMethod]
	public async Task OnGet_PopulatesMovieExtensions()
	{
		var formats = new Dictionary<string, DeprecatedMovieFormat?>
		{
			["bk2"] = new DeprecatedMovieFormat { FileExtension = "bk2", Deprecated = false },
			["fm2"] = new DeprecatedMovieFormat { FileExtension = "fm2", Deprecated = true },
			["zmv"] = null
		};
		_deprecator.GetAll().Returns(formats);

		await _page.OnGet();

		Assert.AreEqual(formats, _page.MovieExtensions);
	}

	[TestMethod]
	public async Task OnPost_InvalidExtension_ReturnsBadRequest()
	{
		_deprecator.IsMovieExtension("invalid").Returns(false);
		var result = await _page.OnPost("invalid", true);
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task OnPost_ValidExtensionDeprecateTrue_CallsDeprecateAndRedirects()
	{
		_deprecator.IsMovieExtension("bk2").Returns(true);
		_deprecator.Deprecate("bk2").Returns(true);

		var result = await _page.OnPost("bk2", true);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("DeprecateMovieFormats", redirect.PageName);

		await _deprecator.Received(1).Deprecate("bk2");
		await _deprecator.DidNotReceive().Allow(Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_ValidExtensionDeprecateFalse_CallsAllowAndReturnsRedirect()
	{
		_deprecator.IsMovieExtension("bk2").Returns(true);
		_deprecator.Allow("bk2").Returns(true);

		var result = await _page.OnPost("bk2", false);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("DeprecateMovieFormats", redirect.PageName);

		await _deprecator.Received(1).Allow("bk2");
		await _deprecator.DidNotReceive().Deprecate(Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_DeprecateSucceeds_SetsSuccessMessage()
	{
		_deprecator.IsMovieExtension("bk2").Returns(true);
		_deprecator.Deprecate("bk2").Returns(true);

		var result = await _page.OnPost("bk2", true);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_AllowSucceeds_SetsSuccessMessage()
	{
		_deprecator.IsMovieExtension("bk2").Returns(true);
		_deprecator.Allow("bk2").Returns(true);

		var result = await _page.OnPost("bk2", false);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_DeprecateFails_SetsErrorMessage()
	{
		_deprecator.IsMovieExtension("bk2").Returns(true);
		_deprecator.Deprecate("bk2").Returns(false);

		var result = await _page.OnPost("bk2", true);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("DeprecateMovieFormats", redirect.PageName);

		await _deprecator.Received(1).Deprecate("bk2");
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_AllowFails_SetsErrorMessage()
	{
		_deprecator.IsMovieExtension("bk2").Returns(true);
		_deprecator.Allow("bk2").Returns(false);

		var result = await _page.OnPost("bk2", false);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("DeprecateMovieFormats", redirect.PageName);

		await _deprecator.Received(1).Allow("bk2");
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}
}
