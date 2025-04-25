using System.Text;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db, IMovieSearchTokens movieTokens) : BasePageModel
{
	[FromQuery]
	public PublicationRequest Paging { get; set; } = new();

	[FromRoute]
	public string Query { get; set; } = "";

	public PageOf<PublicationDisplay, PublicationRequest> Movies { get; set; } = new([], new());

	public async Task<IActionResult> OnGet()
	{
		var tokenLookup = await movieTokens.GetTokens();
		var tokens = Query.ToTokens();
		var searchModel = PublicationSearch.FromTokens(tokens, tokenLookup);

		// If no valid filter criteria, don't attempt to generate a list (else it would be all movies for what is most likely a malformed URL)
		if (searchModel.IsEmpty)
		{
			return BaseRedirect("Movies");
		}

		Movies = await db.Publications
			.FilterByTokens(searchModel)
			.ToViewModel(searchModel.SortBy == "y", User.GetUserId())
			.PageOf(Paging);
		return Page();
	}

	[PagingDefaults(PageSize = 100)]
	public class PublicationRequest : PagingModel;

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
		public IEnumerable<Tag> Tags { get; init; } = [];
		public IEnumerable<string> GameGenres { get; init; } = [];
		public IEnumerable<File> Files { get; init; } = [];
		public IEnumerable<Flag> Flags { get; init; } = [];
		public IEnumerable<ObsoleteMovie> ObsoletedMovies { get; init; } = [];
		public File Screenshot => Files.First(f => f.Type == FileType.Screenshot);

		public IEnumerable<File> MovieFileLinks => Files.Where(f => f.Type == FileType.MovieFile);
		public int RatingCount { get; init; }
		public double? OverallRating { get; init; }
		public CurrentRating Rating { get; init; } = null!;

		public record Tag(string DisplayName, string Code);
		public record Flag(string? IconPath, string? LinkPath, string Name);
		public record File(int Id, string Path, FileType Type, string? Description);
		public record ObsoleteMovie(int Id, string Title);
		public record PublicationUrl(PublicationUrlType Type, string? Url, string? DisplayName);
		public record CurrentRating(string? Rating, bool Unrated);
	}

	public class PublicationSearch : IPublicationTokens
	{
		public ICollection<string> SystemCodes { get; init; } = [];
		public ICollection<string> Classes { get; init; } = [];
		public ICollection<int> Years { get; init; } = [];
		public ICollection<string> Tags { get; init; } = [];
		public ICollection<string> Genres { get; init; } = [];
		public ICollection<string> Flags { get; init; } = [];
		public bool ShowObsoleted { get; init; }
		public bool OnlyObsoleted { get; init; }
		public string SortBy { get; init; } = "";
		public int? Limit { get; init; }

		public ICollection<int> Authors { get; init; } = [];
		public ICollection<int> MovieIds { get; init; } = [];
		public ICollection<int> Games { get; init; } = [];
		public ICollection<int> GameGroups { get; init; } = [];

		public bool IsEmpty => !SystemCodes.Any()
			&& !Classes.Any()
			&& !Years.Any()
			&& !Flags.Any()
			&& !Tags.Any()
			&& !Genres.Any()
			&& !Authors.Any()
			&& !MovieIds.Any()
			&& !Games.Any()
			&& !GameGroups.Any();

		public string ToUrl()
		{
			var sb = new StringBuilder();
			sb.Append(string.Join("-", Classes));
			if (SystemCodes.Any())
			{
				sb.Append('-').Append(string.Join("-", SystemCodes));
			}

			if (Years.Any())
			{
				sb.Append('-').Append(string.Join("-", Years.Select(y => $"Y{y}")));
			}

			if (Tags.Any())
			{
				sb.Append('-').Append(string.Join("-", Tags));
			}

			if (Flags.Any())
			{
				sb.Append('-').Append(string.Join("-", Flags));
			}

			if (Genres.Any())
			{
				sb.Append('-').Append(string.Join("-", Genres));
			}

			if (Games.Any())
			{
				sb.Append('-').Append(string.Join("-", Games.Select(g => $"{g}g")));
			}

			if (GameGroups.Any())
			{
				sb.Append('-').Append(string.Join("-", GameGroups.Select(gg => $"group{gg}")));
			}

			if (Authors.Any())
			{
				sb.Append('-').Append(string.Join("-", Authors.Select(a => $"author{a}")));
			}

			if (OnlyObsoleted && !IsEmpty)
			{
				sb.Append("-ObsOnly");
			}
			else if (ShowObsoleted && !IsEmpty)
			{
				sb.Append("-Obs");
			}

			if (!string.IsNullOrWhiteSpace(SortBy))
			{
				sb.Append("-Sort").Append(SortBy);
			}

			return sb.ToString().Trim('-');
		}

		public static PublicationSearch FromTokens(ICollection<string> tokens, IPublicationTokens tokenLookup)
		{
			var limitStr = tokens
				.Where(t => t.StartsWith("limit"))
				.Select(t => t.Replace("limit", ""))
				.FirstOrDefault();
			int? limit = null;
			if (int.TryParse(limitStr, out int l))
			{
				limit = l;
			}

			return new PublicationSearch
			{
				Classes = tokenLookup.Classes.Where(tokens.Contains).ToList(),
				SystemCodes = tokenLookup.SystemCodes.Where(tokens.Contains).ToList(),
				ShowObsoleted = tokens.Contains("obs"),
				OnlyObsoleted = tokens.Contains("obsonly"),
				SortBy = tokens.Where(t => t.StartsWith("sort")).Select(t => t.Replace("sort", "")).FirstOrDefault() ?? "",
				Limit = limit,
				Years = tokenLookup.Years.Where(y => tokens.Contains("y" + y)).ToList(),
				Tags = tokenLookup.Tags.Where(tokens.Contains).ToList(),
				Genres = tokenLookup.Genres.Where(tokens.Contains).ToList(),
				Flags = tokenLookup.Flags.Where(tokens.Contains).ToList(),
				MovieIds = tokens.ToIdList('m'),
				Games = tokens.ToIdList('g'),
				GameGroups = tokens.ToIdListPrefix("group"),
				Authors = tokens
					.Where(t => t.Contains("author", StringComparison.InvariantCultureIgnoreCase))
					.Select(t => t.ToLower().Replace("author", ""))
					.Select(t => int.TryParse(t, out var temp) ? temp : (int?)null)
					.Where(t => t.HasValue)
					.Select(t => t!.Value)
					.ToList()
			};
		}
	}
}
