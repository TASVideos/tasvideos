namespace TASVideos;

public static class DbContextExtensions
{
	extension(DbContext db)
	{
		public void ExtendTimeoutForSearch()
		{
			db.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
		}

		public bool HasFullTextSearch() => db.Database.IsNpgsql();
	}
}
