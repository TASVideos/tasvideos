using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications.Models;

public class PublicationDisplayModel
{
	public int Id { get; set; }
	public int GameId { get; set; }
	public string GameName { get; set; } = "";
	public DateTime CreateTimestamp { get; set; }
	public DateTime LastUpdateTimestamp { get; set; }
	public int? ObsoletedById { get; set; }
	public string Title { get; set; } = "";
	public string? ClassIconPath { get; set; }
	public string MovieFileName { get; set; } = "";
	public int SubmissionId { get; set; }

	internal ICollection<PublicationUrl> Urls { get; set; } = new List<PublicationUrl>();
	public IEnumerable<PublicationUrl> OnlineWatchingUrls => Urls.Where(u => u.Type == PublicationUrlType.Streaming);
	public IEnumerable<PublicationUrl> MirrorSiteUrls => Urls.Where(u => u.Type == PublicationUrlType.Mirror);
	public int TopicId { get; set; }
	public string? EmulatorVersion { get; set; }

	public IEnumerable<TagModel> Tags { get; set; } = new List<TagModel>();
	public IEnumerable<TagModel> GenreTags { get; set; } = new List<TagModel>();
	public IEnumerable<FileModel> Files { get; set; } = new List<FileModel>();
	public IEnumerable<FlagModel> Flags { get; set; } = new List<FlagModel>();

	public IEnumerable<ObsoletesModel> ObsoletedMovies { get; set; } = new List<ObsoletesModel>();

	public FileModel Screenshot => Files.First(f => f.Type == FileType.Screenshot);

	public IEnumerable<FileModel> MovieFileLinks => Files
		.Where(f => f.Type == FileType.MovieFile);

	public int RatingCount { get; set; }
	public double? OverallRating { get; set; }

	public class TagModel
	{
		public string DisplayName { get; set; } = "";
		public string Code { get; set; } = "";
	}

	public class FileModel
	{
		public int Id { get; set; }
		public string Path { get; set; } = "";
		public FileType Type { get; set; }
		public string? Description { get; set; }
	}

	public class FlagModel
	{
		public string? IconPath { get; set; }
		public string? LinkPath { get; set; }
		public string Name { get; set; } = "";
	}

	public class ObsoletesModel
	{
		public int Id { get; set; }
		public string Title { get; set; } = "";
	}

	public record PublicationUrl(PublicationUrlType Type, string? Url, string? DisplayName);
}
