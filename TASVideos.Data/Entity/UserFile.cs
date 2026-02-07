namespace TASVideos.Data.Entity;

public enum UserFileClass
{
	[Display(Name = "Movie")]
	Movie,

	[Display(Name = "Support file")]
	Support
}

public enum Compression
{
	None = 0,
	Gzip = 1,
	Zip = 2
}

public class UserFile
{
	public long Id { get; set; }

	public int AuthorId { get; set; }
	public User? Author { get; set; }

	public string FileName { get; set; } = "";

	public byte[] Content { get; set; } = [];

	public UserFileClass Class { get; set; }

	public string Type { get; set; } = "";

	public DateTime UploadTimestamp { get; set; }

	public decimal Length { get; set; }

	public int Frames { get; set; }

	public int Rerecords { get; set; }

	public string Title { get; set; } = "";

	public string? Description { get; set; }

	public int LogicalLength { get; set; }

	public int PhysicalLength { get; set; }

	public int? GameId { get; set; }
	public Game.Game? Game { get; set; }

	public int? SystemId { get; set; }
	public GameSystem? System { get; set; }

	public bool Hidden { get; set; }

	public int Downloads { get; set; }

	public Compression CompressionType { get; set; }

	public string? Annotations { get; set; }

	public ICollection<UserFileComment> Comments { get; init; } = [];
}

public static class UserFileExtensions
{
	extension(IQueryable<UserFile> query)
	{
		public IQueryable<UserFile> ThatArePublic() => query.Where(q => !q.Hidden);

		public IQueryable<UserFile> HideIfNotAuthor(int userId)
			=> query.Where(uf => !uf.Hidden || uf.AuthorId == userId);

		public IQueryable<UserFile> ThatAreMovies() => query.Where(q => q.Class == UserFileClass.Movie);

		public IQueryable<UserFile> ByRecentlyUploaded() => query.OrderByDescending(q => q.UploadTimestamp);

		public IQueryable<UserFile> ForAuthor(string userName) => query.Where(q => q.Author!.UserName == userName);

		public IQueryable<UserFile> ForGame(int gameId) => query.Where(q => q.GameId == gameId);
	}

	public static IEnumerable<UserFile> HideIfNotAuthor(this IEnumerable<UserFile> query, int userId)
		=> query.Where(uf => !uf.Hidden || uf.AuthorId == userId);
}
