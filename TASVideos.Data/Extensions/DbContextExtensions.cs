namespace TASVideos;

public static class DbContextExtensions
{
	public static void ExtendTimeoutForSearch(this DbContext db)
	{
		db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
	}

	public static bool HasFullTextSearch(this DbContext db) => db.Database.IsNpgsql();
}
