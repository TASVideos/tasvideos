using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Test.MVC.Tasks
{
	[TestClass]
	public class UserTaskTests
    {
		private UserTasks _userTasks;
		private ApplicationDbContext _db;

		[TestInitialize]
		public void Initialize()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("TestDb")
				.Options;

			_db = new ApplicationDbContext(options, null);
			_db.Database.EnsureDeleted();
		}

		[TestCleanup]
		public void Cleanup()
		{
			_db.Dispose();
		}

	    [TestMethod]
		public async Task UserTasks_GetUserNameById_ValidId_ReturnsCorrectName()
		{
			
			var testName = "TestUser";
			_db.Users.Add(new User { Id = 1, UserName = testName });
			_db.SaveChanges();
			var userTasks = new UserTasks(_db, null, null, new NoCacheService()); // TODO: managers

			var result = await userTasks.GetUserNameById(1);
			Assert.AreEqual(testName, result);
		}

		[TestMethod]
		public async Task UserTasks_GetUserNameById_InvalidId_ReturnsNull()
		{
			_db.Users.Add(new User { Id = 2, UserName = "TestUser" });
			_db.SaveChanges();
			var userTasks = new UserTasks(_db, null, null, new NoCacheService()); // TODO: managers

			var result = await userTasks.GetUserNameById(1);
			Assert.IsNull(result);
		}
    }
}
