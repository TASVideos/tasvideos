using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PointsServiceTests : TestDbBase
{
	private readonly PointsService _pointsService;
	private static string Author => "Player";
	private static string Author2 => "Player2";

	public PointsServiceTests()
	{
		_pointsService = new PointsService(_db, new NoCacheService());
	}

	[TestMethod]
	public async Task PlayerPoints_NoUser_ReturnsZero()
	{
		var (actual, _) = await _pointsService.PlayerPoints(int.MinValue);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task PlayerPoints_UserWithNoMovies_ReturnsZero()
	{
		var user = _db.AddUser(1, Author);
		await _db.SaveChangesAsync();
		var (actual, _) = await _pointsService.PlayerPoints(user.Entity.Id);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task PlayerPoints_NoRatings_MinimumPointsReturned()
	{
		const int numMovies = 2;
		var user = _db.AddUser(1, Author);

		for (int i = 0; i < numMovies; i++)
		{
			_db.AddPublication();
		}

		await _db.SaveChangesAsync();
		var pa = _db.Publications
			.ToList()
			.Select(p => new PublicationAuthor
			{
				PublicationId = p.Id,
				UserId = user.Entity.Id
			})
			.ToList();
		_db.PublicationAuthors.AddRange(pa);
		await _db.SaveChangesAsync();

		var (actual, _) = await _pointsService.PlayerPoints(user.Entity.Id);

		const int expected = numMovies * PlayerPointConstants.MinimumPlayerPointsForPublication;
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task PlayerPoints_OnlyObsoletedPublications_NearZero()
	{
		var user = _db.AddUser(1, Author);

		var newPub = _db.AddPublication().Entity;
		var oldPub = _db.AddPublication(user.Entity).Entity;
		oldPub.ObsoletedBy = newPub;
		await _db.SaveChangesAsync();

		var (actual, _) = await _pointsService.PlayerPoints(user.Entity.Id);
		Assert.IsTrue(actual > 0);
		Assert.IsTrue(actual < 0.5);
	}

	[TestMethod]
	public async Task PlayerPoints_PassesWeightMultiplier()
	{
		// 2 authors, 1 for a non-weighted pub and 1 for a weighted pub
		var author1 = _db.AddUser(1, Author);
		var author2 = _db.AddUser(2, Author2);
		var nonWeightedFlag = new Flag { Id = 1, Name = "Regular", Weight = 1, Token = "regular" };
		var weightedFlag = new Flag { Id = 2, Name = "Weighted", Weight = 100, Token = "weighted" };
		_db.Flags.Add(nonWeightedFlag);
		_db.Flags.Add(weightedFlag);

		var pubNonWeighted = _db.AddPublication(author1.Entity);
		pubNonWeighted.Entity.PublicationFlags.Add(new PublicationFlag { Flag = nonWeightedFlag });
		var pubWeighted = _db.AddPublication(author2.Entity);
		pubWeighted.Entity.PublicationFlags.Add(new PublicationFlag { Flag = weightedFlag });
		await _db.SaveChangesAsync();

		// 3rd User rates each publication equally
		var rater = _db.AddUser(3);
		_db.PublicationRatings.Add(new PublicationRating { Publication = pubNonWeighted.Entity, User = rater.Entity, Value = 10 });
		_db.PublicationRatings.Add(new PublicationRating { Publication = pubWeighted.Entity, User = rater.Entity, Value = 10 });
		await _db.SaveChangesAsync();

		var (actualNonWeighted, _) = await _pointsService.PlayerPoints(author1.Entity.Id);
		var (actualWeighted, _) = await _pointsService.PlayerPoints(author2.Entity.Id);
		Assert.AreEqual(actualNonWeighted * 100, actualWeighted, 10);
	}
}
