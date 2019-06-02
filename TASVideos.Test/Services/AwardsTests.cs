using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;
using TASVideos.Services;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Services
{
	[TestClass]
	public class AwardsTests
	{
		private IAwards _awards;
		private TestDbContext _db;
		private ICacheService _cache;

		private static readonly int CurrentYear = DateTime.UtcNow.Year - 2000;

		[TestInitialize]
		public void Initialize()
		{
			_db = TestDbContext.Create();
			_cache = new NoCacheService();
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
		}

		[TestMethod]
		public async Task ForUser_UserAwardForAnotherUser_ReturnsNoAward()
		{
			var award = CreateAuthorAward();
			var userWithAward = CreateUser();
			GiveUserAnAward(userWithAward, award);

			var userWithNoAward = CreateUser();

			var actual = await _awards.ForUser(userWithNoAward.Id);

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		[TestMethod]
		public async Task ForUser_PublicationAwardForAnotherAuthor_ReturnsNoAward()
		{
			var award = CreatePublicationAward();
			var author = CreateUser();
			var pub = CreatePublication(author);
			GivePublicationAnAward(pub, award);

			var authorWithNoAward = CreateUser();
			CreatePublication(authorWithNoAward);

			var actual = await _awards.ForUser(authorWithNoAward.Id);

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
				ShortName = "UserAward", Description = "User Award", Type = AwardType.User
			};

			_db.Awards.Add(award);
			_db.SaveChanges();
			return award;
		}

		private void GiveUserAnAward(User user, Award award)
		{
			_db.UserAwards.Add(new UserAward { AwardId = award.Id, UserId = user.Id, Year = CurrentYear });
			_db.SaveChanges();
		}

		private Publication CreatePublication(User author)
		{
			var pub = new Publication { Title = "Test Publication" };
			_db.Publications.Add(pub);
			_db.SaveChanges();

			_db.PublicationAuthors.Add(new PublicationAuthor
			{
				PublicationId = pub.Id, UserId = author.Id
			});
			_db.SaveChanges();

			return pub;
		}

		private Award CreatePublicationAward()
		{
			var award = new Award
			{
				ShortName = "PublicationAward", Description = "Publication Award", Type = AwardType.Movie
			};

			_db.Awards.Add(award);
			_db.SaveChanges();
			return award;
		}

		private void GivePublicationAnAward(Publication publication, Award award)
		{
			_db.PublicationAwards.Add(new PublicationAward
			{
				AwardId = award.Id, PublicationId = publication.Id, Year = CurrentYear
			});
			_db.SaveChanges();
		}
	}
}
