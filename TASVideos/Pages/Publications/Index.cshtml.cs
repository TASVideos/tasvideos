using TASVideos.Core;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db, IMovieSearchTokens movieTokens) : BasePageModel
{
	[FromQuery]
	public PublicationRequest Paging { get; set; } = new();

	[FromRoute]
	public string Query { get; set; } = "";

	public PageOf<PublicationDisplay> Movies { get; set; } = PageOf<PublicationDisplay>.Empty();

	public async Task<IActionResult> OnGet()
	{
		var tokenLookup = await movieTokens.GetTokens();
		var tokens = Query.ToTokens();
		var searchModel = PublicationSearchModel.FromTokens(tokens, tokenLookup);

		// If no valid filter criteria, don't attempt to generate a list (else it would be all movies for what is most likely a malformed URL)
		if (searchModel.IsEmpty)
		{
			return BaseRedirect("Movies");
		}

		Movies = await db.Publications
			.FilterByTokens(searchModel)
			.ToViewModel(searchModel.SortBy == "y", User.GetUserId())
			.PageOf(Paging);
		ViewData["ReturnUrl"] = HttpContext.CurrentPathToReturnUrl();
		return Page();
	}

	public class PublicationRequest : PagingModel
	{
		public PublicationRequest() { PageSize = 100; }
	}

	public class PublicationDisplay
	{
		public int Id { get; init; }
		public int GameId { get; init; }
		public string GameName { get; init; } = "";
		public int GameVersionId { get; init; }
		public string GameVersionName { get; init; } = "";
		public DateTime CreateTimestamp { get; init; }
		public DateTime LastUpdateTimestamp { get; init; }
		public int? ObsoletedById { get; init; }
		public string Title { get; init; } = "";
		public string Class { get; init; } = "";
		public string? ClassIconPath { get; init; }
		public string MovieFileName { get; init; } = "";
		public int SubmissionId { get; init; }
		internal IReadOnlyCollection<PublicationUrl> Urls { get; init; } = [];
		public IEnumerable<PublicationUrl> OnlineWatchingUrls => Urls.Where(u => u.Type == PublicationUrlType.Streaming);
		public IEnumerable<PublicationUrl> MirrorSiteUrls => Urls.Where(u => u.Type == PublicationUrlType.Mirror);
		public int TopicId { get; init; }
		public string? EmulatorVersion { get; init; }
		public IEnumerable<TagModel> Tags { get; init; } = [];
		public IEnumerable<string> GameGenres { get; init; } = [];
		public IEnumerable<FileModel> Files { get; init; } = [];
		public IEnumerable<FlagModel> Flags { get; init; } = [];
		public IEnumerable<ObsoletesModel> ObsoletedMovies { get; init; } = [];
		public FileModel Screenshot => Files.First(f => f.Type == FileType.Screenshot);

		public IEnumerable<FileModel> MovieFileLinks => Files.Where(f => f.Type == FileType.MovieFile);
		public int RatingCount { get; init; }
		public double? OverallRating { get; init; }
		public RateModel.RatingDisplay Rating { get; init; } = new();

		public record TagModel(string DisplayName, string Code);

		public record FileModel(int Id, string Path, FileType Type, string? Description);

		public record FlagModel(string? IconPath, string? LinkPath, string Name);

		public record ObsoletesModel(int Id, string Title);

		public record PublicationUrl(PublicationUrlType Type, string? Url, string? DisplayName);
	}
}
