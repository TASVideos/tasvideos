using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Tests.Base;

namespace TASVideos.Core.Tests.Services
{
	[TestClass]
	public class PublicationMaintenanceLoggerTests
	{
		private readonly TestDbContext _db;
		private readonly PublicationMaintenanceLogger _publicationMaintenanceLogger;

		public PublicationMaintenanceLoggerTests()
		{
			_db = TestDbContext.Create();
			_publicationMaintenanceLogger = new PublicationMaintenanceLogger(_db);
		}

		[TestMethod]
		public async Task Log_BasicSuccess()
		{
			const int publicationId = 1;
			const int userId = 2;
			const string logMessage = "Test";
			_db.Publications.Add(new Publication { Id = publicationId });
			_db.Users.Add(new User { Id = userId });
			await _db.SaveChangesAsync();

			await _publicationMaintenanceLogger.Log(publicationId, userId, logMessage);
			var logs = _db.PublicationMaintenanceLogs.ToList();
			Assert.AreEqual(1, logs.Count);
			var log = logs.Single();
			Assert.AreEqual(publicationId, log.PublicationId);
			Assert.AreEqual(userId, log.UserId);
			Assert.AreEqual(logMessage, log.Log);
		}
	}
}
