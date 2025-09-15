using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Pages.Publications;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class RateModelTests : TestDbBase
{
	private readonly IPublications _publications;
	private readonly IRatingService _ratingService;
	private readonly RateModel _page;

	public RateModelTests()
	{
		_publications = Substitute.For<IPublications>();
		_ratingService = Substitute.For<IRatingService>();
		_page = new RateModel(_publications, _ratingService);
	}

	[TestMethod]
	public async Task OnGet_NoPublication_ReturnsNotFound()
	{
		_page.Id = 999;
		_publications.GetTitle(Arg.Any<int>()).Returns((string?)null);
		var actual = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_ValidPublication_PopulatesData()
	{
		const int id = 123;
		const string title = "Test Publication";
		_publications.GetTitle(id).Returns(title);
		_page.Id = id;

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		var userRating = new PublicationRating { Value = 8.5 };
		var ratings = new List<RatingEntry> { new("TestUser", 8.5, true) };
		const double overallRating = 8.2;

		_ratingService.GetUserRatingForPublication(user.Id, id).Returns(userRating);
		_ratingService.GetRatingsForPublication(id).Returns(ratings);
		_ratingService.GetOverallRatingForPublication(id).Returns(overallRating);

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(title, _page.Title);
		Assert.AreEqual("8.5", _page.Rating);
		Assert.AreEqual(1, _page.AllRatings.Count);
		Assert.AreEqual(overallRating, _page.OverallRating);
	}

	[TestMethod]
	public async Task OnGet_UserWithoutPrivateRatingPermission_OnlySeesPublicRatings()
	{
		const int id = 123;
		_page.Id = id;

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		var ratings = new List<RatingEntry>
		{
			new("TestUser", 8.5, true),
			new("OtherUser", 6.0, false),
			new("AnotherUser", 7.0, true)
		};

		_ratingService.GetRatingsForPublication(id).Returns(ratings);

		await _page.OnGet();

		var visibleRatings = _page.VisibleRatings.ToList();
		Assert.AreEqual(2, visibleRatings.Count);
		Assert.IsTrue(visibleRatings.All(r => r.IsPublic || r.UserName == "TestUser"));
	}

	[TestMethod]
	public async Task OnGet_UserWithPrivateRatingPermission_SeesAllRatings()
	{
		const int id = 123;
		_page.Id = id;

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies, PermissionTo.SeePrivateRatings]);

		var ratings = new List<RatingEntry>
		{
			new("TestUser", 8.5, true),
			new("OtherUser", 6.0, false),
			new("AnotherUser", 7.0, true)
		};

		_ratingService.GetRatingsForPublication(id).Returns(ratings);

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
		const int id = 123;
		_page.Id = id;
		_page.Rating = "8.5";

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		_ratingService.UpdateUserRating(user.Id, id, 8.5).Returns(SaveResult.Success);

		var result = await _page.OnPost();

		AssertRedirect(result, "/Publications/Rate", id);
		await _ratingService.Received(1).UpdateUserRating(user.Id, id, 8.5);
	}

	[TestMethod]
	public async Task OnPost_RatingServiceFails_ShowsErrorMessage()
	{
		const int id = 123;
		_page.Id = id;
		_page.Rating = "8.5";

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		_ratingService.UpdateUserRating(user.Id, id, 8.5).Returns(SaveResult.UpdateFailure);

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		await _ratingService.Received(1).UpdateUserRating(user.Id, id, 8.5);
	}

	[TestMethod]
	public async Task OnPostInline_NullRating_ClearsRating()
	{
		const int id = 123;
		_page.Id = id;

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.RateMovies]);

		const string nullRatingJson = "null";
		var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(nullRatingJson));
		_page.HttpContext.Request.Body = stream;

		const double overallRating = 7.8;
		_ratingService.GetOverallRatingForPublication(id).Returns(overallRating);

		var actual = await _page.OnPostInline();

		Assert.IsInstanceOfType<JsonResult>(actual);
		await _ratingService.Received(1).UpdateUserRating(user.Id, id, null);
		await _ratingService.Received(1).GetOverallRatingForPublication(id);
	}
}
