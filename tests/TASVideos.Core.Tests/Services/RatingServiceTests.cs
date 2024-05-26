using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class RatingServiceTests
{
	private readonly TestDbContext _db;
	private readonly RatingService _ratingService;

	public RatingServiceTests()
	{
		_db = TestDbContext.Create();
		_ratingService = new RatingService(_db);
	}

	[TestMethod]
	public async Task GetUserRatingForPublication_ReturnsNullIfNoRating()
	{
		var actual = await _ratingService.GetUserRatingForPublication(0, 0);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetUserRatingForPublication_ReturnsRating()
	{
		const int pubId = 1;
		const string userName = "User";
		const double rating = 5.5;
		_db.Publications.Add(new Publication { Id = pubId });
		var user = _db.AddUser(userName).Entity;
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = 1, User = user, Value = rating });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetUserRatingForPublication(user.Id, pubId);
		Assert.IsNotNull(actual);
		Assert.AreEqual(pubId, actual.PublicationId);
		Assert.AreEqual(user.Id, actual.UserId);
		Assert.AreEqual(rating, actual.Value);
	}

	[TestMethod]
	public async Task GetRatingsForPublication_ReturnsCorrectRatings()
	{
		const int pubId = 1;
		const string userName1 = "User 1";
		const string userName2 = "User 2";

		var user1 = _db.AddUser(userName1).Entity;
		var user2 = _db.AddUser(userName2).Entity;
		_db.Publications.Add(new Publication { Id = pubId });

		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pubId, User = user1, Value = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pubId, User = user2, Value = 2 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetRatingsForPublication(pubId);

		Assert.IsNotNull(actual);
		Assert.AreEqual(actual.Count, 2);
		Assert.AreEqual(1, actual.Count(pr => pr.UserName == userName1));
		Assert.AreEqual(1, actual.Count(pr => pr.UserName == userName2));
	}

	[TestMethod]
	public async Task GetOverallRatingForPublication_NoPublication_ReturnsZero()
	{
		var actual = await _ratingService.GetOverallRatingForPublication(0);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetOverallRatingForPublication_ReturnsCorrectAverage()
	{
		const int pubId = 1;
		const string userName1 = "User 1";
		const string userName2 = "User 2";

		var user1 = _db.AddUser(userName1).Entity;
		var user2 = _db.AddUser(userName2).Entity;
		_db.Publications.Add(new Publication { Id = pubId });

		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pubId, User = user1, Value = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pubId, User = user2, Value = 9 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetOverallRatingForPublication(pubId);
		Assert.AreEqual(5, actual);
	}

	[TestMethod]
	public async Task GetOverallRatingForPublication_DoesNotUseAuthorRatings()
	{
		const int pubId = 1;
		const string userName1 = "User 1";
		const string userName2 = "User 2";

		var user1 = _db.AddUser(userName1).Entity;
		var user2 = _db.AddUser(userName2).Entity;
		_db.Publications.Add(new Publication { Id = pubId });
		_db.PublicationAuthors.Add(new PublicationAuthor { UserId = user2.Id, PublicationId = pubId });

		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pubId, User = user1, Value = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pubId, User = user2, Value = 9 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetOverallRatingForPublication(pubId);
		Assert.AreEqual(1, actual);
	}

	[TestMethod]
	public async Task GetOverallRatingForPublication_DoesNotUseIfUserDoesNotHaveUseRatings()
	{
		const int pubId = 1;
		const string userName1 = "User 1";
		const string userName2 = "User 2";

		var user1 = _db.AddUser(userName1).Entity;
		var user2 = _db.AddUser(userName2).Entity;
		user2.UseRatings = false;
		_db.Publications.Add(new Publication { Id = pubId });

		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pubId, User = user1, Value = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pubId, User = user2, Value = 9 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetOverallRatingForPublication(pubId);
		Assert.AreEqual(1, actual);
	}

	[TestMethod]
	public async Task UpdateUserRating_RatingHasValueAndUserAlreadyRated_UpdatesRating()
	{
		const int pubId = 1;
		const string userName = "User";
		const double rating = 5.5;
		_db.Publications.Add(new Publication { Id = pubId });
		var user = _db.AddUser(userName).Entity;
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = 1, User = user, Value = rating });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.UpdateUserRating(user.Id, pubId, rating + 1);
		Assert.AreEqual(SaveResult.Success, actual);
		var savedRating = _db.PublicationRatings.Single(pr => pr.PublicationId == pubId);
		Assert.AreEqual(rating + 1, savedRating.Value);
	}

	[TestMethod]
	public async Task UpdateUserRating_RatingIsNullAndUserAlreadyRated_RemovesRating()
	{
		const int pubId = 1;
		const string userName = "User";
		const double rating = 5.5;
		_db.Publications.Add(new Publication { Id = pubId });
		var user = _db.AddUser(userName).Entity;
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = 1, User = user, Value = rating });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.UpdateUserRating(user.Id, pubId, null);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.AreEqual(0, _db.PublicationRatings.Count());
	}

	[TestMethod]
	public async Task UpdateUserRating_UserNotYetRated_AddsRatingAndRoundsIt()
	{
		const int pubId = 1;
		const string userName = "User";
		_db.Publications.Add(new Publication { Id = pubId });
		var user = _db.AddUser(userName).Entity;
		await _db.SaveChangesAsync();

		var actual = await _ratingService.UpdateUserRating(user.Id, pubId, 5.222);
		Assert.AreEqual(SaveResult.Success, actual);
		var savedRating = _db.PublicationRatings.Single(pr => pr.PublicationId == pubId);
		Assert.AreEqual(5.2, savedRating.Value);
	}

	[TestMethod]
	public async Task UpdateUserRating_UserNotYetRatedAndRatingIsNull_SavesNothing()
	{
		const int pubId = 1;
		const string userName = "User";
		_db.Publications.Add(new Publication { Id = pubId });
		var user = _db.AddUser(userName).Entity;
		await _db.SaveChangesAsync();

		var actual = await _ratingService.UpdateUserRating(user.Id, pubId, null);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.AreEqual(0, _db.PublicationRatings.Count());
	}
}
