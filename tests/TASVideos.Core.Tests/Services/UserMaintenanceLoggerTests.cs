using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class UserMaintenanceLoggerTests
{
	private readonly TestDbContext _db;
	private readonly UserMaintenanceLogger _publicationMaintenanceLogger;

	public UserMaintenanceLoggerTests()
	{
		_db = TestDbContext.Create();
		_publicationMaintenanceLogger = new UserMaintenanceLogger(_db);
	}

	[TestMethod]
	public async Task Log_BasicSuccess()
	{
		const int userId = 1;
		const int editorId = 2;
		const string logMessage = "Test";
		_db.Users.Add(new User { Id = userId });
		_db.Users.Add(new User { Id = editorId });
		await _db.SaveChangesAsync();

		await _publicationMaintenanceLogger.Log(userId, logMessage, editorId);
		var logs = _db.UserMaintenanceLogs.ToList();
		Assert.AreEqual(1, logs.Count);
		var log = logs.Single();
		Assert.AreEqual(userId, log.UserId);
		Assert.AreEqual(editorId, log.EditorId);
		Assert.AreEqual(logMessage, log.Log);
	}
}
