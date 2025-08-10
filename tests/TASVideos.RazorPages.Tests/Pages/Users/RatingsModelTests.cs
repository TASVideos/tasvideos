using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Pages.Users;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Users;

[TestClass]
public class RatingModelTests : TestDbBase
{
	private readonly IRatingService _ratingService;
	private readonly RatingsModel _page;

	public RatingModelTests()
	{
		_ratingService = Substitute.For<IRatingService>();
		_page = new RatingsModel(_ratingService);
	}

	[TestMethod]
	public async Task UserDoesNotExist_ReturnsNotFound()
	{
		const string userName = "DoesNotExist";
		_page.UserName = userName;
		_ratingService.GetUserRatings(userName, _page.Search).Returns(null as UserRatings);

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task UserExists_PrivateRatings_ViewerDoesNotHavePermission_ReturnsNotFound()
	{
		const string userName = "RaterExists";
		var viewer = _db.AddUser("Viewer");
		SetRatingResult(userName);
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_page, viewer.Entity, []);

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task UserExists_PrivateRatings_ViewerHasPermission_ReturnsRatings()
	{
		const string userName = "RaterExists";
		SetRatingResult(userName);
		var viewer = _db.AddUser("Viewer");
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_page, viewer.Entity, [PermissionTo.SeePrivateRatings]);

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _page.Ratings.Ratings.Count());
	}

	[TestMethod]
	public async Task UserExists_PrivateRatings_ViewerIsUserAndWithoutPermission_ReturnsRatings()
	{
		const string userName = "RaterExists";
		var user = _db.AddUser(userName);
		SetRatingResult(userName);
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_page, user.Entity, []);

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _page.Ratings.Ratings.Count());
	}

	private void SetRatingResult(string userName)
	{
		_page.UserName = userName;
		_ratingService.GetUserRatings(userName, _page.Search, true).Returns(new UserRatings
		{
			Ratings = new PageOf<UserRatings.Rating, RatingRequest>([new()], _page.Search)
		});
		_ratingService.GetUserRatings(userName, _page.Search, false).Returns(null as UserRatings);
	}
}
