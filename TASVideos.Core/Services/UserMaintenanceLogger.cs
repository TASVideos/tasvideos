namespace TASVideos.Core.Services;

public interface IUserMaintenanceLogger
{
	Task Log(int userId, string log, int? editorId = null);
}

internal class UserMaintenanceLogger(ApplicationDbContext db) : IUserMaintenanceLogger
{
	public async Task Log(int userId, string log, int? editorId = null)
	{
		editorId ??= SiteGlobalConstants.TASVideoAgentId;

		db.UserMaintenanceLogs.Add(new UserMaintenanceLog
		{
			UserId = userId,
			EditorId = editorId,
			Log = log,
			TimeStamp = DateTime.UtcNow
		});
		await db.SaveChangesAsync();
	}
}
