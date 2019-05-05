using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Services
{
	[TestClass]
	public class PointsServiceTests
	{
		private IPointsService _pointsService;
		private ApplicationDbContext _db;
		private static readonly User Player = new User { UserName = "Player" };

		[TestInitialize]
		public void Initialize()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("TestDb")
				.Options;
			_db = new ApplicationDbContext(options, null);
			_db.Database.EnsureDeleted();
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
			_db.SaveChanges();
			var user = _db.Users.Single();
			var actual = await _pointsService.PlayerPoints(user.Id);
			Assert.AreEqual(0, actual);
		}

		[TestMethod]
		public async Task PlayerPoints_NoRatings_MinimumPointsReturned()
		{
			int numMovies = 2;

			_db.Users.Add(Player);
			var tier = new Tier { Weight = 1, Name = "Test" };
			_db.Tiers.Add(tier);
			for (int i = 0; i < numMovies; i++)
			{
				_db.Publications.Add(new Publication { Tier = tier });
			}
			
			_db.SaveChanges();
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
			_db.SaveChanges();

			var actual = await _pointsService.PlayerPoints(user.Id);
			int expected = numMovies * PlayerPointConstants.MinimumPlayerPointsForPublication;
			Assert.AreEqual(expected, actual);
		}
	}
}
