using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class RatingServiceTests : TestDbBase
{
	private readonly RatingService _ratingService;

	public RatingServiceTests()
	{
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
		const string userName = "User";
		const double rating = 5.5;
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();
		var user = _db.AddUser(userName).Entity;
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user, Value = rating });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetUserRatingForPublication(user.Id, pub.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(pub.Id, actual.PublicationId);
		Assert.AreEqual(user.Id, actual.UserId);
		Assert.AreEqual(rating, actual.Value);
	}

	[TestMethod]
	public async Task GetRatingsForPublication_ReturnsCorrectRatings()
	{
		const string userName1 = "User 1";
		const string userName2 = "User 2";

		var user1 = _db.AddUser(userName1).Entity;
		var user2 = _db.AddUser(userName2).Entity;
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();

		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user1, Value = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user2, Value = 2 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetRatingsForPublication(pub.Id);

		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual.Count);
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
	public async Task GetOverallRatingForPublication_PublicationWithNoRatings_ReturnsZero()
	{
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetOverallRatingForPublication(pub.Id);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetOverallRatingForPublication_ReturnsCorrectAverage()
	{
		const string userName1 = "User 1";
		const string userName2 = "User 2";

		var user1 = _db.AddUser(userName1).Entity;
		var user2 = _db.AddUser(userName2).Entity;
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();

		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user1, Value = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user2, Value = 9 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetOverallRatingForPublication(pub.Id);
		Assert.AreEqual(5, actual);
	}

	[TestMethod]
	public async Task GetOverallRatingForPublication_DoesNotUseAuthorRatings()
	{
		const string userName1 = "User 1";
		const string userName2 = "User 2";

		var user1 = _db.AddUser(userName1).Entity;
		var user2 = _db.AddUser(userName2).Entity;
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();
		_db.PublicationAuthors.Add(new PublicationAuthor { UserId = user2.Id, PublicationId = pub.Id });

		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user1, Value = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user2, Value = 9 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetOverallRatingForPublication(pub.Id);
		Assert.AreEqual(1, actual);
	}

	[TestMethod]
	public async Task GetOverallRatingForPublication_DoesNotUseIfUserDoesNotHaveUseRatings()
	{
		const string userName1 = "User 1";
		const string userName2 = "User 2";

		var user1 = _db.AddUser(userName1).Entity;
		var user2 = _db.AddUser(userName2).Entity;
		user2.UseRatings = false;
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();

		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user1, Value = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user2, Value = 9 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetOverallRatingForPublication(pub.Id);
		Assert.AreEqual(1, actual);
	}

	[TestMethod]
	public async Task UpdateUserRating_RatingHasValueAndUserAlreadyRated_UpdatesRating()
	{
		const string userName = "User";
		const double rating = 5.5;
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();
		var user = _db.AddUser(userName).Entity;
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user, Value = rating });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.UpdateUserRating(user.Id, pub.Id, rating + 1);
		Assert.AreEqual(SaveResult.Success, actual);
		var savedRating = _db.PublicationRatings.Single(pr => pr.PublicationId == pub.Id);
		Assert.AreEqual(rating + 1, savedRating.Value);
	}

	[TestMethod]
	public async Task UpdateUserRating_RatingIsNullAndUserAlreadyRated_RemovesRating()
	{
		const string userName = "User";
		const double rating = 5.5;
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();
		var user = _db.AddUser(userName).Entity;
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = pub.Id, User = user, Value = rating });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.UpdateUserRating(user.Id, pub.Id, null);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.AreEqual(0, _db.PublicationRatings.Count());
	}

	[TestMethod]
	public async Task UpdateUserRating_UserNotYetRated_AddsRatingAndRoundsIt()
	{
		const string userName = "User";
		var pub = _db.AddPublication().Entity;
		var user = _db.AddUser(userName).Entity;
		await _db.SaveChangesAsync();

		var actual = await _ratingService.UpdateUserRating(user.Id, pub.Id, 5.222);
		Assert.AreEqual(SaveResult.Success, actual);
		var savedRating = _db.PublicationRatings.Single(pr => pr.PublicationId == pub.Id);
		Assert.AreEqual(5.2, savedRating.Value);
	}

	[TestMethod]
	public async Task UpdateUserRating_UserNotYetRatedAndRatingIsNull_SavesNothing()
	{
		const string userName = "User";
		var pub = _db.AddPublication().Entity;
		var user = _db.AddUser(userName).Entity;
		await _db.SaveChangesAsync();

		var actual = await _ratingService.UpdateUserRating(user.Id, pub.Id, null);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.AreEqual(0, _db.PublicationRatings.Count());
	}

	[TestMethod]
	public async Task GetUserRatings_UserDoesNotExist_ReturnsNull()
	{
		var actual = await _ratingService.GetUserRatings("DoesNotExist", new RatingRequest());
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetUserRatings_UserExistsButHidesRatings_IncludeHiddenFalse_ReturnsNoRatings()
	{
		const string userName = "a";
		var user = _db.AddUser(userName);
		user.Entity.PublicRatings = false;
		var pub = _db.AddPublication();
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub.Entity, User = user.Entity, Value = 1.0 });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetUserRatings(userName, new RatingRequest(), includeHidden: false);
		Assert.IsNotNull(actual);
		Assert.AreEqual(userName, actual.UserName);
		Assert.IsFalse(actual.PublicRatings);
		Assert.AreEqual(0, actual.Ratings.RowCount);
	}

	[TestMethod]
	public async Task GetUserRatings_UserExistsButHidesRatings_IncludeHiddenTrue_ReturnsRatings()
	{
		const string userName = "a";
		var user = _db.AddUser(userName);
		user.Entity.PublicRatings = false;
		const double ratingValue = 1.0;
		var pub = _db.AddPublication();
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub.Entity, User = user.Entity, Value = ratingValue });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetUserRatings(userName, new RatingRequest(), includeHidden: true);
		Assert.IsNotNull(actual);
		Assert.AreEqual(userName, actual.UserName);
		Assert.IsFalse(actual.PublicRatings);
		Assert.AreEqual(1, actual.Ratings.RowCount);
		var actualRating = actual.Ratings.First();
		Assert.AreEqual(pub.Entity.Id, actualRating.PublicationId);
		Assert.AreEqual(ratingValue, actualRating.Value);
	}

	[TestMethod]
	public async Task GetUserRatings_UserExistsWithPublicRatings_IncludeHiddenFalse_ReturnsRatings()
	{
		const string userName = "a";
		var user = _db.AddUser(userName);
		user.Entity.PublicRatings = true;
		const double ratingValue = 1.0;
		var pub = _db.AddPublication();
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub.Entity, User = user.Entity, Value = ratingValue });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetUserRatings(userName, new RatingRequest(), includeHidden: false);
		Assert.IsNotNull(actual);
		Assert.AreEqual(userName, actual.UserName);
		Assert.IsTrue(actual.PublicRatings);
		Assert.AreEqual(1, actual.Ratings.RowCount);
		var actualRating = actual.Ratings.First();
		Assert.AreEqual(pub.Entity.Id, actualRating.PublicationId);
		Assert.AreEqual(ratingValue, actualRating.Value);
	}

	[TestMethod]
	public async Task GetUserRatings_DoNotIncludeObsolete_DoesNotReturnRatingForObsoletePublication()
	{
		const string userName = "a";
		var user = _db.AddUser(userName);
		var pub = _db.AddPublication();
		var obsoletePub = _db.AddPublication();
		obsoletePub.Entity.ObsoletedBy = pub.Entity;

		const double ratingValue = 2.0;
		const double ratingValueForObs = 1.0;
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub.Entity, User = user.Entity, Value = ratingValue });
		_db.PublicationRatings.Add(new PublicationRating { Publication = obsoletePub.Entity, User = user.Entity, Value = ratingValueForObs });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetUserRatings(userName, new RatingRequest { IncludeObsolete = false });
		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.Ratings.RowCount);
		var actualRating = actual.Ratings.First();
		Assert.AreEqual(pub.Entity.Id, actualRating.PublicationId);
		Assert.AreEqual(ratingValue, actualRating.Value);
	}

	[TestMethod]
	public async Task GetUserRatings_IncludeObsolete_ReturnsRatingForObsoletePublication()
	{
		const string userName = "a";
		var user = _db.AddUser(userName);
		var pub = _db.AddPublication();
		var obsoletePub = _db.AddPublication();
		obsoletePub.Entity.ObsoletedBy = pub.Entity;

		const double ratingValue = 2.0;
		const double ratingValueForObs = 1.0;
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub.Entity, User = user.Entity, Value = ratingValue });
		_db.PublicationRatings.Add(new PublicationRating { Publication = obsoletePub.Entity, User = user.Entity, Value = ratingValueForObs });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetUserRatings(userName, new RatingRequest { IncludeObsolete = true });
		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual.Ratings.RowCount);
	}

	[TestMethod]
	public async Task GetUserRatings_Summary_ReturnsSensibleValues()
	{
		var userA = _db.AddUser("A").Entity;
		var userB = _db.AddUser("B").Entity;
		var userC = _db.AddUser("C").Entity;
		var userD = _db.AddUser("D").Entity;
		_db.AddUser("E");
		var pub1 = _db.AddPublication().Entity;
		var pub2 = _db.AddPublication().Entity;
		var pub3 = _db.AddPublication().Entity;
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub1, User = userA });
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub2, User = userA });
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub3, User = userA });

		_db.PublicationRatings.Add(new PublicationRating { Publication = pub1, User = userB });
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub2, User = userB });

		_db.PublicationRatings.Add(new PublicationRating { Publication = pub1, User = userC });
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub2, User = userC });

		_db.PublicationRatings.Add(new PublicationRating { Publication = pub1, User = userD });
		await _db.SaveChangesAsync();

		var actual = await _ratingService.GetUserRatings(userB.UserName, new RatingRequest());
		Assert.IsNotNull(actual);
		Assert.AreEqual(3, actual.Summary.TotalPublicationCount);
		Assert.AreEqual(4, actual.Summary.TotalRaterCount);
		Assert.AreEqual(2, actual.RatingsCount);
		Assert.AreEqual(1, actual.UsersWithHigherRatingsCount);
		Assert.AreEqual(1, actual.UsersWithEqualRatingsCount);
		Assert.AreEqual(1, actual.UsersWithLowerRatingsCount);
	}
}
