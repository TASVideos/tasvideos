namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PublicationMaintenanceLoggerTests : TestDbBase
{
	private readonly PublicationMaintenanceLogger _publicationMaintenanceLogger;

	public PublicationMaintenanceLoggerTests()
	{
		_publicationMaintenanceLogger = new PublicationMaintenanceLogger(_db);
	}

	[TestMethod]
	public async Task Log_BasicSuccess()
	{
		const int userId = 2;
		const string logMessage = "Test";
		var pub = _db.AddPublication().Entity;
		_db.AddUser(userId, "_");
		await _db.SaveChangesAsync();

		await _publicationMaintenanceLogger.Log(pub.Id, userId, logMessage);
		var logs = _db.PublicationMaintenanceLogs.ToList();
		Assert.AreEqual(1, logs.Count);
		var log = logs.Single();
		Assert.AreEqual(pub.Id, log.PublicationId);
		Assert.AreEqual(userId, log.UserId);
		Assert.AreEqual(logMessage, log.Log);
	}

	[TestMethod]
	public async Task Log_Multiple_BasicSuccess()
	{
		const int userId = 2;
		const string message1 = "Test1";
		const string message2 = "Test2";
		var pub = _db.AddPublication().Entity;
		_db.AddUser(userId, "_");
		await _db.SaveChangesAsync();

		await _publicationMaintenanceLogger.Log(pub.Id, userId, [message1, message2]);
		var logs = _db.PublicationMaintenanceLogs.ToList();
		Assert.AreEqual(2, logs.Count);
		Assert.IsTrue(logs.Any(l => l.PublicationId == pub.Id && l.UserId == userId && l.Log == message1));
		Assert.IsTrue(logs.Any(l => l.PublicationId == pub.Id && l.UserId == userId && l.Log == message2));
	}
}
