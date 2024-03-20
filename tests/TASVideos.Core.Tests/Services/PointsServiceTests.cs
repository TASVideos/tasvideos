﻿using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PointsServiceTests
{
	private readonly PointsService _pointsService;
	private readonly TestDbContext _db;
	private static string Author => "Player";

	public PointsServiceTests()
	{
		_db = TestDbContext.Create();
		_pointsService = new PointsService(_db, new NoCacheService());
	}

	[TestMethod]
	public async Task PlayerPoints_NoUser_Returns0()
	{
		var (actual, _) = await _pointsService.PlayerPoints(int.MinValue);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task PlayerPoints_UserWithNoMovies_Returns0()
	{
		_db.AddUser(1, Author);
		await _db.SaveChangesAsync();
		var user = _db.Users.Single();
		var (actual, _) = await _pointsService.PlayerPoints(user.Id);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task PlayerPoints_NoRatings_MinimumPointsReturned()
	{
		const int numMovies = 2;

		_db.AddUser(1, Author);
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

		var (actual, _) = await _pointsService.PlayerPoints(user.Id);
		const int expected = numMovies * PlayerPointConstants.MinimumPlayerPointsForPublication;
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task PlayerPoints_OnlyObsoletedPublications_NonZero()
	{
		_db.AddUser(1, Author);
		var publicationClass = new PublicationClass { Weight = 1, Name = "Test" };
		_db.PublicationClasses.Add(publicationClass);
		await _db.SaveChangesAsync();

		var user = _db.Users.Single();
		var newPub = new Publication();
		var oldPub = new Publication
		{
			Authors = [new PublicationAuthor { UserId = user.Id }],
			ObsoletedBy = newPub,
			PublicationClass = publicationClass,
		};
		_db.Publications.Add(oldPub);
		_db.Publications.Add(newPub);
		await _db.SaveChangesAsync();

		var (actual, _) = await _pointsService.PlayerPoints(user.Id);
		Assert.IsTrue(actual > 0);
	}
}
