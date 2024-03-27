namespace TASVideos.Pages.Publications.Models;

public class PublicationDisplayModel
{
	public int Id { get; set; }
	public int GameId { get; set; }
	public string GameName { get; set; } = "";
	public int GameVersionId { get; set; }
	public string GameVersionName { get; set; } = "";
	public DateTime CreateTimestamp { get; set; }
	public DateTime LastUpdateTimestamp { get; set; }
	public int? ObsoletedById { get; set; }
	public string Title { get; set; } = "";
	public string Class { get; set; } = "";
	public string? ClassIconPath { get; set; }
	public string MovieFileName { get; set; } = "";
	public int SubmissionId { get; set; }

	internal IReadOnlyCollection<PublicationUrl> Urls { get; set; } = [];
	public IEnumerable<PublicationUrl> OnlineWatchingUrls => Urls.Where(u => u.Type == PublicationUrlType.Streaming);
	public IEnumerable<PublicationUrl> MirrorSiteUrls => Urls.Where(u => u.Type == PublicationUrlType.Mirror);
	public int TopicId { get; set; }
	public string? EmulatorVersion { get; set; }

	public IEnumerable<TagModel> Tags { get; set; } = [];
	public IEnumerable<string> GameGenres { get; set; } = [];
	public IEnumerable<FileModel> Files { get; set; } = [];
	public IEnumerable<FlagModel> Flags { get; set; } = [];

	public IEnumerable<ObsoletesModel> ObsoletedMovies { get; set; } = [];

	public FileModel Screenshot => Files.First(f => f.Type == FileType.Screenshot);

	public IEnumerable<FileModel> MovieFileLinks => Files
		.Where(f => f.Type == FileType.MovieFile);

	public int RatingCount { get; set; }
	public double? OverallRating { get; set; }
	public PublicationRateModel Rating { get; set; } = new();

	public record TagModel(string DisplayName, string Code);

	public record FileModel(int Id, string Path, FileType Type, string? Description);

	public record FlagModel(string? IconPath, string? LinkPath, string Name);

	public record ObsoletesModel(int Id, string Title);

	public record PublicationUrl(PublicationUrlType Type, string? Url, string? DisplayName);
}
