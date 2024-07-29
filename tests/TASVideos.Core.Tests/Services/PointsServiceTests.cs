using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PointsServiceTests : TestDbBase
{
	private readonly PointsService _pointsService;
	private static string Author => "Player";

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
			_db.Publications.Add(new Publication());
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

		var newPub = new Publication();
		var oldPub = new Publication
		{
			Authors = [new PublicationAuthor { UserId = user.Entity.Id }],
			ObsoletedBy = newPub
		};
		_db.Publications.Add(oldPub);
		_db.Publications.Add(newPub);
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
		var author2 = _db.AddUser(2, Author);
		var nonWeightedFlag = new Flag { Id = 1, Name = "Regular", Weight = 1 };
		var weightedFlag = new Flag { Id = 2, Name = "Weighted", Weight = 100 };
		_db.Flags.Add(nonWeightedFlag);
		_db.Flags.Add(weightedFlag);
		var pubNonWeighted = _db.Publications.Add(new Publication
		{
			Authors = [new PublicationAuthor { UserId = author1.Entity.Id }],
			PublicationFlags = [new PublicationFlag { Flag = nonWeightedFlag }]
		});
		var pubWeighted = _db.Publications.Add(new Publication
		{
			Authors = [new PublicationAuthor { UserId = author2.Entity.Id }],
			PublicationFlags = [new PublicationFlag { Flag = weightedFlag }]
		});
		await _db.SaveChangesAsync();

		// 3rd User rates each publication equally
		var rater = _db.AddUser(3);
		_db.PublicationRatings.Add(new PublicationRating { Publication = pubNonWeighted.Entity, User = rater.Entity, Value = 10 });
		_db.PublicationRatings.Add(new PublicationRating { Publication = pubWeighted.Entity, User = rater.Entity, Value = 10 });
		await _db.SaveChangesAsync();

		var (actualNonWeighted, _) = await _pointsService.PlayerPoints(author1.Entity.Id);
		var (actualWeighted, _) = await _pointsService.PlayerPoints(author2.Entity.Id);
		Assert.AreEqual(actualWeighted, actualNonWeighted * 100, 10);
	}
}
