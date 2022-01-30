using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

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

	[TestMethod]
	public async Task Log_Multiple_BasicSuccess()
	{
		const int publicationId = 1;
		const int userId = 2;
		const string message1 = "Test1";
		const string message2 = "Test2";
		_db.Publications.Add(new Publication { Id = publicationId });
		_db.Users.Add(new User { Id = userId });
		await _db.SaveChangesAsync();

		await _publicationMaintenanceLogger.Log(publicationId, userId, new[] { message1, message2 });
		var logs = _db.PublicationMaintenanceLogs.ToList();
		Assert.AreEqual(2, logs.Count);
		Assert.IsTrue(logs.Any(l => l.PublicationId == publicationId
			&& l.UserId == userId
			&& l.Log == message1));
		Assert.IsTrue(logs.Any(l => l.PublicationId == publicationId
			&& l.UserId == userId
			&& l.Log == message2));
	}
}
