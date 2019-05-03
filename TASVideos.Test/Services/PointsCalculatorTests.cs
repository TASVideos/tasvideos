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
	public class PointsCalculatorTests
	{
		private IPointsCalculator _pointsCalculator;
		private ApplicationDbContext _db;

		[TestInitialize]
		public void Initialize()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("TestDb")
				.Options;
			_db = new ApplicationDbContext(options, null);
			_db.Database.EnsureDeleted();
			_pointsCalculator = new PointsCalculator(_db, new NoCacheService());
		}

		[TestMethod]
		public async Task PlayerPoints_NoUser_Returns0()
		{
			var actual = await _pointsCalculator.PlayerPoints(int.MinValue);
			Assert.AreEqual(0, actual);
		}

		[TestMethod]
		public async Task PlayerPoints_UserWithNoMovies_Returns0()
		{
			_db.Users.Add(new User { UserName = "TestUser" });
			_db.SaveChanges();
			var user = _db.Users.Single();
			var actual = await _pointsCalculator.PlayerPoints(user.Id);
			Assert.AreEqual(0, actual);
		}
	}
}
