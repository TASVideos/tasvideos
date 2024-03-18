namespace TASVideos.Core.Services;

public interface IPublicationMaintenanceLogger
{
	Task Log(int publicationId, int userId, string log);
	Task Log(int publicationId, int userId, IEnumerable<string> logs);
}

internal class PublicationMaintenanceLogger(ApplicationDbContext db) : IPublicationMaintenanceLogger
{
	public async Task Log(int publicationId, int userId, string log)
	{
		Add(publicationId, userId, log);
		await db.SaveChangesAsync();
	}

	public async Task Log(int publicationId, int userId, IEnumerable<string> logs)
	{
		foreach (var log in logs)
		{
			Add(publicationId, userId, log);
		}

		await db.SaveChangesAsync();
	}

	private void Add(int publicationId, int userId, string log)
	{
		db.PublicationMaintenanceLogs.Add(new PublicationMaintenanceLog
		{
			TimeStamp = DateTime.UtcNow,
			PublicationId = publicationId,
			UserId = userId,
			Log = log
		});
	}
}
