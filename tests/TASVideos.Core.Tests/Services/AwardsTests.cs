using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class AwardsTests
{
	private readonly IAwards _awards;
	private readonly TestDbContext _db;
	private readonly TestCache _cache;

	private static readonly int CurrentYear = DateTime.UtcNow.Year;

	public AwardsTests()
	{
		_db = TestDbContext.Create();
		_cache = new TestCache();
		_awards = new Awards(_db, _cache);
	}

	[TestMethod]
	public async Task ForUser_UserDoesNotExist_ReturnsEmptyList()
	{
		var actual = await _awards.ForUser(int.MaxValue);

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task ForUser_UserAward_ReturnsAward()
	{
		var award = CreateAuthorAward();
		var user = CreateUser();
		GiveUserAnAward(user, award);

		var actual = await _awards.ForUser(user.Id);

		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(1, list.Count);
		var actualUserAward = list.Single();
		Assert.AreEqual(award.ShortName, actualUserAward.ShortName);
		Assert.AreEqual(CurrentYear, actualUserAward.Year);
		Assert.IsTrue(actualUserAward.Description.Contains(CurrentYear.ToString()));
	}

	[TestMethod]
	public async Task ForUser_AuthorOfPublicationWithAward_ReturnsAward()
	{
		var award = CreatePublicationAward();
		var author = CreateUser();
		var pub = CreatePublication(author);
		GivePublicationAnAward(pub, award);

		var actual = await _awards.ForUser(author.Id);

		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(1, list.Count);
		var actualUserAward = list.Single();
		Assert.AreEqual(award.ShortName, actualUserAward.ShortName);
		Assert.AreEqual(CurrentYear, actualUserAward.Year);
		Assert.IsTrue(actualUserAward.Description.Contains(CurrentYear.ToString()));
	}

	[TestMethod]
	public async Task ForUser_PublicationAwardForAnotherAuthor_ReturnsNoAward()
	{
		var authorAward = CreateAuthorAward();
		var publicationAward = CreatePublicationAward();
		var author = CreateUser();
		var pub = CreatePublication(author);
		GivePublicationAnAward(pub, publicationAward);
		GiveUserAnAward(author, authorAward);

		var authorWithNoAward = CreateUser();
		CreatePublication(authorWithNoAward);

		var actual = await _awards.ForUser(authorWithNoAward.Id);

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task ForPublication_PublicationDoesNotExist_ReturnsEmptyList()
	{
		var actual = await _awards.ForPublication(int.MaxValue);

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task ForPublication_PublicationAward_ReturnsAward()
	{
		var user = CreateUser();
		var pub = CreatePublication(user);
		var award = CreatePublicationAward();
		GivePublicationAnAward(pub, award);

		var actual = await _awards.ForPublication(pub.Id);

		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(1, list.Count);
		var actualPubAward = list.Single();
		Assert.AreEqual(award.ShortName, actualPubAward.ShortName);
		Assert.AreEqual(CurrentYear, actualPubAward.Year);
		Assert.IsTrue(actualPubAward.Description.Contains(CurrentYear.ToString()));
	}

	[TestMethod]
	public async Task ForPublication_PublicationAwardForPublication_ReturnsNoAward()
	{
		var publicationAward = CreatePublicationAward();
		var author = CreateUser();
		var pub = CreatePublication(author);
		GivePublicationAnAward(pub, publicationAward);

		var pubWithNoAward = CreatePublication(author);

		var actual = await _awards.ForPublication(pubWithNoAward.Id);

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task ForUser_AuthorOfTwoPublicationsWithIdenticalAward_ReturnsTwoAwards()
	{
		var publicationAward = CreatePublicationAward();
		var author = CreateUser();
		var pub1 = CreatePublication(author);
		var pub2 = CreatePublication(author);
		GivePublicationAnAward(pub1, publicationAward);
		GivePublicationAnAward(pub2, publicationAward);

		var actual = await _awards.ForUser(author.Id);

		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual.Count());
	}

	[TestMethod]
	public async Task ForPublication_AuthorHasAWard_ReturnsNoAward()
	{
		var author = CreateUser();
		var pub = CreatePublication(author);
		var award = CreateAuthorAward();
		GiveUserAnAward(author, award);

		var actual = await _awards.ForPublication(pub.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task ForYear_YearDoesNotExist_ReturnsEmptyList()
	{
		var actual = await _awards.ForYear(int.MaxValue);
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task ForYear_UserAward_ReturnsAward()
	{
		var award = CreatePublicationAward();
		var author = CreateUser();
		var pub = CreatePublication(author);
		GivePublicationAnAward(pub, award);

		var actual = await _awards.ForYear(CurrentYear);

		// Award should match
		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(1, list.Count);
		var actualAward = list.Single();
		Assert.AreEqual(award.ShortName, actualAward.ShortName);

		// Publication should match
		Assert.IsNotNull(actualAward.Publications);
		var actualPublications = actualAward.Publications.ToList();
		Assert.AreEqual(1, actualPublications.Count);
		Assert.AreEqual(pub.Id, actualPublications.Single().Id);
	}

	[TestMethod]
	public async Task ForYear_PublicationAward_ReturnsAward()
	{
		var award = CreateAuthorAward();
		var user = CreateUser();
		GiveUserAnAward(user, award);

		var actual = await _awards.ForYear(CurrentYear);

		// Award should match
		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(1, list.Count);
		var actualAward = list.Single();
		Assert.AreEqual(award.ShortName, actualAward.ShortName);

		// User should match
		Assert.IsNotNull(actualAward.Users);
		var actualUsers = actualAward.Users.ToList();
		Assert.AreEqual(1, actualUsers.Count);
		Assert.AreEqual(user.Id, actualUsers.Single().Id);
	}

	[TestMethod]
	public async Task ForYear_AwardsForAnotherYear_ReturnsNoAward()
	{
		var userAward = CreateAuthorAward();
		var pubAward = CreatePublicationAward();
		var author = CreateUser();
		var pub = CreatePublication(author);
		GiveUserAnAward(author, userAward);
		GivePublicationAnAward(pub, pubAward);

		var actual = await _awards.ForYear(CurrentYear - 1);

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task AddAwardCategory_ShortNameExists_ReturnsFalse()
	{
		const string existingAward = "existing_short_name";
		_db.Awards.Add(new Award { ShortName = existingAward });
		await _db.SaveChangesAsync();

		var actual = await _awards.AddAwardCategory(AwardType.User, existingAward, "");
		Assert.IsFalse(actual);
		Assert.AreEqual(1, _db.Awards.Count());
	}

	[TestMethod]
	public async Task AddAwardCategory_Successful_ReturnsTrue()
	{
		const AwardType awardType = AwardType.User;
		const string shortName = "short_name";
		const string description = "Description";

		var actual = await _awards.AddAwardCategory(awardType, shortName, description);

		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.Awards.Count());
		var actualAward = _db.Awards.Single();
		Assert.AreEqual(awardType, actualAward.Type);
		Assert.AreEqual(shortName, actualAward.ShortName);
		Assert.AreEqual(description, actualAward.Description);
	}

	[TestMethod]
	[DataRow("short_name", "short_name", true)]
	[DataRow("Short_name", "short_name", true)]
	[DataRow("short_name", "shortname", false)]
	public async Task CategoryExists(string shortName, string requestedShortName, bool expected)
	{
		_db.Awards.Add(new Award { ShortName = shortName });
		await _db.SaveChangesAsync();

		var actual = await _awards.CategoryExists(requestedShortName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task AssignUserAward_NotFound_throws()
	{
		await _awards.AssignUserAward("DoesNotExist", 1, Enumerable.Empty<int>());
	}

	[TestMethod]
	public async Task AssignUserAward_Successful_FlushesCache()
	{
		var user = CreateUser();
		var award = CreateAuthorAward();
		_cache.Set(CacheKeys.AwardsCache, new List<AwardAssignment> { new() { ShortName = "PreviouslyCached" } });

		int year = DateTime.UtcNow.Year - 1;
		await _awards.AssignUserAward(
			award.ShortName,
			year,
			new[] { user.Id });

		Assert.AreEqual(1, _db.UserAwards.Count());
		Assert.IsTrue(_cache.ContainsKey(CacheKeys.AwardsCache));
		var cache = _cache.Get<List<AwardAssignmentSummary>>(CacheKeys.AwardsCache);
		Assert.AreEqual(1, cache.Count);
		var cachedAward = cache.Single();
		Assert.AreEqual(award.ShortName, cachedAward.ShortName);
		Assert.AreEqual(year, cachedAward.Year);
	}

	[TestMethod]
	public async Task AssignUserAward_IgnoredExistingAssignments()
	{
		var author = CreateUser();
		var award = CreateAuthorAward();
		GiveUserAnAward(author, award);

		await _awards.AssignUserAward(
			award.ShortName,
			CurrentYear,
			new[] { author.Id });

		Assert.AreEqual(1, _db.UserAwards.Count());
		Assert.IsTrue(_cache.ContainsKey(CacheKeys.AwardsCache));
		var cache = _cache.Get<List<AwardAssignmentSummary>>(CacheKeys.AwardsCache);
		Assert.AreEqual(1, cache.Count);
		var cachedAward = cache.Single();
		Assert.AreEqual(award.ShortName, cachedAward.ShortName);
		Assert.AreEqual(CurrentYear, cachedAward.Year);
	}

	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task AssignPublicationAward_NotFound_throws()
	{
		await _awards.AssignPublicationAward("DoesNotExist", 1, Enumerable.Empty<int>());
	}

	[TestMethod]
	public async Task AssignPublicationAward_Successful_FlushesCache()
	{
		var author = CreateUser();
		var pub = CreatePublication(author);
		var award = CreatePublicationAward();
		_cache.Set(CacheKeys.AwardsCache, new List<AwardAssignment> { new() { ShortName = "PreviouslyCached" } });

		int year = DateTime.UtcNow.Year - 1;
		await _awards.AssignPublicationAward(
			award.ShortName,
			year,
			new[] { pub.Id });

		Assert.AreEqual(1, _db.PublicationAwards.Count());
		Assert.IsTrue(_cache.ContainsKey(CacheKeys.AwardsCache));
		var cache = _cache.Get<List<AwardAssignmentSummary>>(CacheKeys.AwardsCache);
		Assert.AreEqual(1, cache.Count);
		var cachedAward = cache.Single();
		Assert.AreEqual(award.ShortName, cachedAward.ShortName);
		Assert.AreEqual(year, cachedAward.Year);
	}

	[TestMethod]
	public async Task AssignPublicationAward_IgnoredExistingAssignments()
	{
		var author = CreateUser();
		var pub = CreatePublication(author);
		var award = CreatePublicationAward();
		GivePublicationAnAward(pub, award);

		await _awards.AssignPublicationAward(
			award.ShortName,
			CurrentYear,
			new[] { pub.Id });

		Assert.AreEqual(1, _db.PublicationAwards.Count());
		Assert.IsTrue(_cache.ContainsKey(CacheKeys.AwardsCache));
		var cache = _cache.Get<List<AwardAssignmentSummary>>(CacheKeys.AwardsCache);
		Assert.AreEqual(1, cache.Count);
		var cachedAward = cache.Single();
		Assert.AreEqual(award.ShortName, cachedAward.ShortName);
		Assert.AreEqual(CurrentYear, cachedAward.Year);
	}

	[TestMethod]
	public async Task Revoke_Success_FlushesCache()
	{
		_cache.Set(CacheKeys.AwardsCache, new object());
		var user = CreateUser();
		var award = CreateAuthorAward();
		GiveUserAnAward(user, award);

		var assignment = new AwardAssignment
		{
			ShortName = award.ShortName,
			Year = CurrentYear,
			Users = new List<AwardAssignment.User>
			{
				new(user.Id, user.UserName)
			}
		};

		await _awards.Revoke(assignment);

		Assert.AreEqual(0, _db.UserAwards.Count());
		Assert.AreEqual(0, _db.PublicationAwards.Count());
		Assert.IsTrue(_cache.ContainsKey(CacheKeys.AwardsCache));
		var awardCache = _cache.Get<List<AwardAssignment>>(CacheKeys.AwardsCache);
		Assert.AreEqual(0, awardCache.Count);
	}

	private User CreateUser()
	{
		var user = new User { UserName = "TestUser" + Guid.NewGuid() };
		_db.Users.Add(user);
		_db.SaveChanges();
		return user;
	}

	private Award CreateAuthorAward()
	{
		var award = new Award
		{
			ShortName = "UserAward",
			Description = "User Award",
			Type = AwardType.User
		};

		_db.Awards.Add(award);
		_db.SaveChanges();
		return award;
	}

	private void GiveUserAnAward(User user, Award award)
	{
		_db.UserAwards.Add(new UserAward
		{
			AwardId = award.Id,
			UserId = user.Id,
			Year = CurrentYear
		});
		_db.SaveChanges();
	}

	private Publication CreatePublication(User author)
	{
		var pub = new Publication { Title = "Test Publication" };
		_db.Publications.Add(pub);
		_db.SaveChanges();

		_db.PublicationAuthors.Add(new PublicationAuthor
		{
			PublicationId = pub.Id,
			UserId = author.Id
		});
		_db.SaveChanges();

		return pub;
	}

	private Award CreatePublicationAward()
	{
		var award = new Award
		{
			ShortName = "PublicationAward",
			Description = "Publication Award",
			Type = AwardType.Movie
		};

		_db.Awards.Add(award);
		_db.SaveChanges();
		return award;
	}

	private void GivePublicationAnAward(Publication publication, Award award)
	{
		_db.PublicationAwards.Add(new PublicationAward
		{
			AwardId = award.Id,
			PublicationId = publication.Id,
			Year = CurrentYear
		});
		_db.SaveChanges();
	}
}
