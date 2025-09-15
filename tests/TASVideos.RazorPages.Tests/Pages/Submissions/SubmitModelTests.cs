using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Submissions;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class SubmitModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IWikiPages _wikiPages;
	private readonly IUserManager _userManager;
	private readonly IMovieFormatDeprecator _movieFormatDeprecator;
	private readonly IQueueService _queueService;
	private SubmitModel _page;

	public SubmitModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_wikiPages = Substitute.For<IWikiPages>();
		_userManager = Substitute.For<IUserManager>();
		_movieFormatDeprecator = Substitute.For<IMovieFormatDeprecator>();
		_queueService = Substitute.For<IQueueService>();
		_page = new SubmitModel(_userManager, _movieFormatDeprecator, _queueService, _wikiPages, _publisher);
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
		_page = new SubmitModel(_userManager, _movieFormatDeprecator, _queueService, _wikiPages, _publisher)
		{
			MovieFile = GenerateTooLargeMovie(),
			Authors = [existingUser]
		};

		await _page.OnPost();

		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.Keys.Contains(nameof(_page.MovieFile)));
	}

	[TestMethod]
	public async Task OnPost_SuccessfulSubmission_CallsServiceAndRedirects()
	{
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		parseResult.FileExtension.Returns("bk2");

		_queueService.ParseMovieFileOrZip(Arg.Any<IFormFile>()).Returns((parseResult, new byte[] { 1, 2, 3 }));
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_userManager.Exists("TestUser").Returns(true);
		_movieFormatDeprecator.IsDeprecated(".bk2").Returns(false);
		_queueService.ExceededSubmissionLimit(user.Id).Returns((DateTime?)null);
		_queueService.Submit(Arg.Any<SubmitRequest>()).Returns(new SubmitResult(null, 42, "", null));

		_page = new SubmitModel(_userManager, _movieFormatDeprecator, _queueService, _wikiPages, _publisher)
		{
			GameName = "Test Game",
			RomName = "test.nes",
			Markup = "Test submission content",
			AgreeToInstructions = true,
			AgreeToLicense = true,
			MovieFile = CreateMockMovieFile(),
			Authors = ["TestUser"]
		};
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirectResult = (RedirectResult)result;
		Assert.AreEqual("/42S", redirectResult.Url);

		// Verify QueueService was called with correct parameters
		await _queueService.Received(1).Submit(Arg.Is<SubmitRequest>(req =>
			req.GameName == "Test Game"
			&& req.RomName == "test.nes"
			&& req.Markup == "Test submission content"
			&& req.Authors.Contains("TestUser")
			&& req.Submitter == user));

		// Verify announcement was sent
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_ServiceThrowsException_ReturnsPageWithError()
	{
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		parseResult.FileExtension.Returns("bk2");

		_queueService.ParseMovieFileOrZip(Arg.Any<IFormFile>()).Returns((parseResult, new byte[] { 1, 2, 3 }));
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_userManager.Exists("TestUser").Returns(true); // Add this line to make validation pass
		_movieFormatDeprecator.IsDeprecated(".bk2").Returns(false);
		_queueService.ExceededSubmissionLimit(user.Id).Returns((DateTime?)null);
		_queueService.Submit(Arg.Any<SubmitRequest>())
			.Returns(new FailedSubmitResult("Database error occurred"));

		_page = new SubmitModel(_userManager, _movieFormatDeprecator, _queueService, _wikiPages, _publisher)
		{
			GameName = "Test Game",
			RomName = "test.nes",
			Markup = "Test submission content",
			AgreeToInstructions = true,
			AgreeToLicense = true,
			MovieFile = CreateMockMovieFile(),
			Authors = ["TestUser"]
		};
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(_page.ModelState.ContainsKey(""));
		var errors = _page.ModelState[""]!.Errors;
		Assert.IsTrue(errors.Count > 0);
		Assert.AreEqual("Database error occurred", errors[0].ErrorMessage);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(SubmitModel), PermissionTo.SubmitMovies);

	private static IFormFile CreateMockMovieFile()
	{
		const string content = "Mock movie file content";
		const string fileName = "test.bk2";
		var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

		return new FormFile(ms, 0, ms.Length, "MovieFile", fileName)
		{
			Headers = new HeaderDictionary(),
			ContentType = "application/octet-stream"
		};
	}

	private static IFormFile GenerateTooLargeMovie()
	{
		byte[] bytes = [.. Enumerable.Repeat<byte>(0xFF, SiteGlobalConstants.MaximumMovieSize + 1)];
		var ms = new MemoryStream(bytes);
		return new FormFile(ms, 0, bytes.Length, "Data", "too-large.bk2");
	}
}
