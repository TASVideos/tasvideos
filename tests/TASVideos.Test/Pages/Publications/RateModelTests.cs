using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class RateModelTests : TestDbBase
{
	private readonly IRatingService _ratingService;
	private readonly RateModel _page;

	public RateModelTests()
	{
		_ratingService = Substitute.For<IRatingService>();
		_page = new RateModel(_db, _ratingService);
	}

	[TestMethod]
	public async Task OnGet_NoPublication_ReturnsNotFound()
	{
		_page.Id = 999;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_ValidPublication_PopulatesData()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		var userRating = new PublicationRating { Value = 8.5 };
		var ratings = new List<RatingEntry> { new("TestUser", 8.5, true) };
		const double overallRating = 8.2;

		_ratingService.GetUserRatingForPublication(user.Id, pub.Id).Returns(userRating);
		_ratingService.GetRatingsForPublication(pub.Id).Returns(ratings);
		_ratingService.GetOverallRatingForPublication(pub.Id).Returns(overallRating);

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(pub.Title, _page.Title);
		Assert.AreEqual("8.5", _page.Rating);
		Assert.AreEqual(1, _page.AllRatings.Count);
		Assert.AreEqual(overallRating, _page.OverallRating);
	}

	[TestMethod]
	public async Task OnGet_UserWithoutPrivateRatingPermission_OnlySeesPublicRatings()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		var ratings = new List<RatingEntry>
		{
			new("TestUser", 8.5, true),
			new("OtherUser", 6.0, false),
			new("AnotherUser", 7.0, true)
		};

		_ratingService.GetRatingsForPublication(pub.Id).Returns(ratings);

		await _page.OnGet();

		var visibleRatings = _page.VisibleRatings.ToList();
		Assert.AreEqual(2, visibleRatings.Count);
		Assert.IsTrue(visibleRatings.All(r => r.IsPublic || r.UserName == "TestUser"));
	}

	[TestMethod]
	public async Task OnGet_UserWithPrivateRatingPermission_SeesAllRatings()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies, PermissionTo.SeePrivateRatings]);

		var ratings = new List<RatingEntry>
		{
			new("TestUser", 8.5, true),
			new("OtherUser", 6.0, false),
			new("AnotherUser", 7.0, true)
		};

		_ratingService.GetRatingsForPublication(pub.Id).Returns(ratings);

		await _page.OnGet();

		var visibleRatings = _page.VisibleRatings.ToList();
		Assert.AreEqual(3, visibleRatings.Count);
		Assert.AreEqual(ratings.Count, visibleRatings.Count);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_page.ModelState.AddModelError("Rating", "Invalid rating");

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
	}

	[TestMethod]
	public async Task OnPost_ValidRating_UpdatesRatingAndRedirects()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;
		_page.Title = pub.Title;
		_page.Rating = "8.5";

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		_ratingService.UpdateUserRating(user.Id, pub.Id, 8.5)
			.Returns(Task.FromResult(SaveResult.Success));

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("/Publications/Rate", redirectResult.PageName);
		Assert.AreEqual(pub.Id, redirectResult.RouteValues!["Id"]);

		await _ratingService.Received(1).UpdateUserRating(user.Id, pub.Id, 8.5);
	}

	[TestMethod]
	public async Task OnPost_RatingServiceFails_ShowsErrorMessage()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;
		_page.Title = pub.Title;
		_page.Rating = "8.5";

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		_ratingService.UpdateUserRating(user.Id, pub.Id, 8.5)
			.Returns(Task.FromResult(SaveResult.UpdateFailure));

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		await _ratingService.Received(1).UpdateUserRating(user.Id, pub.Id, 8.5);
	}

	[TestMethod]
	public async Task OnPostInline_NullRating_ClearsRating()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		var nullRatingJson = "null";
		var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(nullRatingJson));
		_page.HttpContext.Request.Body = stream;

		const double overallRating = 7.8;
		_ratingService.GetOverallRatingForPublication(pub.Id)
			.Returns(Task.FromResult(overallRating));

		var actual = await _page.OnPostInline();

		Assert.IsInstanceOfType<JsonResult>(actual);
		await _ratingService.Received(1).UpdateUserRating(user.Id, pub.Id, null);
		await _ratingService.Received(1).GetOverallRatingForPublication(pub.Id);
	}
}
