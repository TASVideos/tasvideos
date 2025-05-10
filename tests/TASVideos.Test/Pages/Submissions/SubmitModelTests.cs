using System.Reflection;
using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;
using TASVideos.Pages;
using TASVideos.Pages.Submissions;
using TASVideos.Services;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class SubmitModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IWikiPages _wikiPages;
	private readonly IMovieParser _movieParser;
	private readonly IUserManager _userManager;
	private readonly ITASVideoAgent _tasVideoAgent;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IMovieFormatDeprecator _movieFormatDeprecator;
	private readonly IFileService _fileService;
	private readonly IQueueService _queueService;
	private SubmitModel _page;

	public SubmitModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_wikiPages = Substitute.For<IWikiPages>();
		_movieParser = Substitute.For<IMovieParser>();
		_userManager = Substitute.For<IUserManager>();
		_tasVideoAgent = Substitute.For<ITASVideoAgent>();
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_movieFormatDeprecator = Substitute.For<IMovieFormatDeprecator>();
		_queueService = Substitute.For<IQueueService>();
		_fileService = Substitute.For<IFileService>();
		_page = new SubmitModel(_db, _publisher, _wikiPages, _movieParser, _userManager, _tasVideoAgent, _youtubeSync, _movieFormatDeprecator, _queueService, _fileService);
	}

	[TestMethod]
	public void RequiresPermission()
	{
		var attribute = typeof(SubmitModel).GetCustomAttribute(typeof(RequirePermissionAttribute));
		Assert.IsNotNull(attribute);
		Assert.IsInstanceOfType<RequirePermissionAttribute>(attribute);
		var permissionAttribute = (RequirePermissionAttribute)attribute;
		Assert.IsTrue(permissionAttribute.RequiredPermissions.Contains(PermissionTo.SubmitMovies));
	}

	[TestMethod]
	public async Task OnGet_LimitExceeded_ReturnsWarningWithNextWindow()
	{
		var user = new User { Id = 1, UserName = "Submitter" };
		var nextWindow = DateTime.UtcNow.AddDays(1);
		AddAuthenticatedUser(_page, user, []);
		_queueService.ExceededSubmissionLimit(user.Id).Returns(nextWindow);

		var actual = await _page.OnGet();

		Assert.IsNotNull(actual);
		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.IsNotNull(redirectResult.RouteValues);
		var nextWindowRouteValue = redirectResult.RouteValues["NextWindow"];
		Assert.IsNotNull(nextWindowRouteValue);
		Assert.IsInstanceOfType<DateTime>(nextWindowRouteValue);
		var nextWindowRouteValueAsDateTime = (DateTime)nextWindowRouteValue;
		Assert.AreEqual(nextWindow, nextWindowRouteValueAsDateTime);
	}

	[TestMethod]
	public async Task OnGet_AddsUserAsAuthorByDefault()
	{
		var user = new User { Id = 1, UserName = "Submitter" };
		AddAuthenticatedUser(_page, user, []);

		var actual = await _page.OnGet();
		Assert.IsInstanceOfType<PageResult>(actual);
	}

	[TestMethod]
	public async Task OnPost_LimitExceeded_ReturnsWarningWithNextWindow()
	{
		var nextWindow = DateTime.UtcNow.AddDays(1);

		var user = new User { Id = 1, UserName = "Submitter" };
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_page, user, []);
		_queueService.ExceededSubmissionLimit(user.Id).Returns(nextWindow);

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.IsNotNull(redirectResult.RouteValues);
		var nextWindowRouteValue = redirectResult.RouteValues["NextWindow"];
		Assert.IsNotNull(nextWindowRouteValue);
		Assert.IsInstanceOfType<DateTime>(nextWindowRouteValue);
		var nextWindowRouteValueAsDateTime = (DateTime)nextWindowRouteValue;
		Assert.AreEqual(nextWindow, nextWindowRouteValueAsDateTime);
	}

	[TestMethod]
	public async Task OnPost_AtLeastOneAuthorRequired()
	{
		_page.Authors = [];

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.Keys.Contains(nameof(_page.Authors)));
	}

	[TestMethod]
	public async Task OnPost_AllAuthorsMustBeUsers()
	{
		const string existingUser = "Exists";
		const string nonexistentUser = "Nonexistent";
		_userManager.Exists(existingUser).Returns(true);
		_userManager.Exists(nonexistentUser).Returns(false);

		_page.Authors = [existingUser, nonexistentUser];

		await _page.OnPost();

		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.Keys.Contains(nameof(_page.Authors)));
		Assert.IsTrue(_page.ModelState.Where(ms => ms.Value is not null).SelectMany(ms => ms.Value!.Errors).Any(ms => ms.ErrorMessage.Contains(nonexistentUser)));
	}

	[TestMethod]
	public async Task OnPost_FileMustBeUnderLimit_IfUserDoesNotHavePermission()
	{
		const string existingUser = "Exists";
		_db.AddUser(existingUser);
		await _db.SaveChangesAsync();
		_page = new SubmitModel(_db, _publisher, _wikiPages, _movieParser, _userManager, _tasVideoAgent, _youtubeSync, _movieFormatDeprecator, _queueService, _fileService)
		{
			MovieFile = GenerateTooLargeMovie(),
			Authors = [existingUser]
		};

		await _page.OnPost();

		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.Keys.Contains(nameof(_page.MovieFile)));
	}

	private static IFormFile GenerateTooLargeMovie()
	{
		byte[] bytes = [.. Enumerable.Repeat<byte>(0xFF, SiteGlobalConstants.MaximumMovieSize + 1)];
		var ms = new MemoryStream(bytes);
		return new FormFile(ms, 0, bytes.Length, "Data", "too-large.bk2");
	}
}
