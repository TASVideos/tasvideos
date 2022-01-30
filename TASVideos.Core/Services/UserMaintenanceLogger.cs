using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public interface IUserMaintenanceLogger
{
	Task Log(int userId, string log, int? editorId = null);
}

internal class UserMaintenanceLogger : IUserMaintenanceLogger
{
	private readonly ApplicationDbContext _db;

	public UserMaintenanceLogger(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task Log(int userId, string log, int? editorId = null)
	{
		editorId ??= SiteGlobalConstants.TASVideoAgentId;

		_db.UserMaintenanceLogs.Add(new UserMaintenanceLog()
		{
			UserId = userId,
			EditorId = editorId,
			Log = log,
			TimeStamp = DateTime.UtcNow
		});
		await _db.SaveChangesAsync();
	}
}
