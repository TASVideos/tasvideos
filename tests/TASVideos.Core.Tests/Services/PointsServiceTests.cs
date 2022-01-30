using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Test.Services;

[TestClass]
public class PointsServiceTests
{
	private readonly IPointsService _pointsService;
	private readonly ApplicationDbContext _db;
	private static User Player => new () { UserName = "Player" };

	public PointsServiceTests()
	{
		_db = TestDbContext.Create();
		_pointsService = new PointsService(_db, new NoCacheService());
	}

	[TestMethod]
	public async Task PlayerPoints_NoUser_Returns0()
	{
		var actual = await _pointsService.PlayerPoints(int.MinValue);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task PlayerPoints_UserWithNoMovies_Returns0()
	{
		_db.Users.Add(Player);
		await _db.SaveChangesAsync();
		var user = _db.Users.Single();
		var actual = await _pointsService.PlayerPoints(user.Id);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task PlayerPoints_NoRatings_MinimumPointsReturned()
	{
		int numMovies = 2;

		_db.Users.Add(Player);
		var publicationClass = new PublicationClass { Weight = 1, Name = "Test" };
		_db.PublicationClasses.Add(publicationClass);
		for (int i = 0; i < numMovies; i++)
		{
			_db.Publications.Add(new Publication { PublicationClass = publicationClass });
		}

		await _db.SaveChangesAsync();
		var user = _db.Users.Single();
		var pa = _db.Publications
			.ToList()
			.Select(p => new PublicationAuthor
			{
				PublicationId = p.Id,
				UserId = user.Id
			})
			.ToList();

		_db.PublicationAuthors.AddRange(pa);
		await _db.SaveChangesAsync();

		var actual = await _pointsService.PlayerPoints(user.Id);
		int expected = numMovies * PlayerPointConstants.MinimumPlayerPointsForPublication;
		Assert.AreEqual(expected, actual);
	}
}
