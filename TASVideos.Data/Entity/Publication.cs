using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity;

/// <summary>
/// Represents filter criteria for filtering publications.
/// </summary>
public interface IPublicationTokens
{
	ICollection<string> SystemCodes { get; }
	ICollection<string> Classes { get; }
	ICollection<int> Years { get; }
	ICollection<string> Tags { get; }
	ICollection<string> Genres { get; }
	ICollection<string> Flags { get; }
	ICollection<int> Authors { get; }
	ICollection<int> MovieIds { get; }
	ICollection<int> Games { get; }
	ICollection<int> GameGroups { get; }
	bool ShowObsoleted { get; }
	bool OnlyObsoleted { get; }
	string SortBy { get; }
	int? Limit { get; }
}

[IncludeInAutoHistory]
public class Publication : BaseEntity, ITimeable
{
	public int Id { get; set; }

	public ICollection<PublicationFile> Files { get; init; } = [];
	public ICollection<PublicationTag> PublicationTags { get; init; } = [];
	public ICollection<PublicationFlag> PublicationFlags { get; init; } = [];
	public ICollection<PublicationAward> PublicationAwards { get; init; } = [];
	public ICollection<PublicationMaintenanceLog> PublicationMaintenanceLogs { get; init; } = [];

	[ForeignKey(nameof(PublicationRating.PublicationId))]
	public ICollection<PublicationRating> PublicationRatings { get; init; } = [];

	public ICollection<PublicationUrl> PublicationUrls { get; init; } = [];

	public int? ObsoletedById { get; set; }
	public Publication? ObsoletedBy { get; set; }

	public ICollection<Publication> ObsoletedMovies { get; init; } = [];

	public int GameId { get; set; }
	public Game.Game? Game { get; set; }

	public int SystemId { get; set; }
	public GameSystem? System { get; set; }

	public int SystemFrameRateId { get; set; }
	public GameSystemFrameRate? SystemFrameRate { get; set; }

	public int GameVersionId { get; set; }
	public GameVersion? GameVersion { get; set; }

	public int PublicationClassId { get; set; }
	public PublicationClass? PublicationClass { get; set; }

	public int SubmissionId { get; set; }
	public Submission? Submission { get; set; }
	public ICollection<PublicationAuthor> Authors { get; init; } = [];

	[ExcludeFromAutoHistory]
	public byte[] MovieFile { get; set; } = [];

	public string MovieFileName { get; set; } = "";

	public string? EmulatorVersion { get; set; }

	public int Frames { get; set; }
	public int RerecordCount { get; set; }

	/// <summary>
	/// Gets or sets Any author's that are not a user. If they are a user, they should be linked, and not listed here.
	/// </summary>
	public string? AdditionalAuthors { get; set; }

	// De-normalized name for easy recreation
	public string Title { get; set; } = "";

	double ITimeable.FrameRate => SystemFrameRate?.FrameRate ?? throw new InvalidOperationException($"{nameof(SystemFrameRate)} must not be lazy loaded!");

	public int? GameGoalId { get; set; }
	public GameGoal? GameGoal { get; set; }

	public void GenerateTitle()
	{
		var authorList = Authors
			.OrderBy(sa => sa.Ordinal)
			.Select(sa => sa.Author?.UserName)
			.Where(sa => !string.IsNullOrWhiteSpace(sa));

		if (!string.IsNullOrWhiteSpace(AdditionalAuthors))
		{
			authorList = authorList.Concat(AdditionalAuthors.SplitWithEmpty(","));
		}

		if (System is null)
		{
			throw new InvalidOperationException($"{nameof(System)} must not be lazy loaded!");
		}

		if (Game is null)
		{
			throw new InvalidOperationException($"{nameof(Game)} must not be lazy loaded!");
		}

		var gameName = Game.DisplayName;
		if (GameVersion is not null && !string.IsNullOrWhiteSpace(GameVersion.TitleOverride))
		{
			gameName = GameVersion.TitleOverride;
		}

		string goal = GameGoal!.DisplayName;
		if (goal == "baseline")
		{
			goal = "";
		}

		Title =
			$"{System.Code} {gameName}"
			+ (!string.IsNullOrWhiteSpace(goal) ? $" \"{goal}\"" : "")
			+ $" by {string.Join(", ", authorList).LastCommaToAmpersand()}"
			+ $" in {this.Time().ToStringWithOptionalDaysAndHours()}";
	}
}

public static class PublicationExtensions
{
	public static IQueryable<Publication> ThatAreCurrent(this IQueryable<Publication> query)
		=> query.Where(p => p.ObsoletedById == null);

	public static IEnumerable<Publication> ThatAreCurrent(this IEnumerable<Publication> query)
		=> query.Where(p => p.ObsoletedById == null);

	public static IQueryable<Publication> ThatAreObsolete(this IQueryable<Publication> query)
		=> query.Where(p => p.ObsoletedById != null);

	public static IQueryable<Publication> ForYearRange(this IQueryable<Publication> query, int before, int after)
		=> query
			.Where(p => p.CreateTimestamp.Year < before)
			.Where(p => p.CreateTimestamp.Year >= after);

	public static IQueryable<Publication> ForDateRange(this IQueryable<Publication> query, DateTime before, DateTime after)
		=> query
			.Where(p => p.CreateTimestamp < before)
			.Where(p => p.CreateTimestamp >= after);

	public static IQueryable<Publication> ThatHaveBeenPublishedBy(this IQueryable<Publication> query, int userId)
		=> query.Where(p => p.Submission!.PublisherId == userId);

	public static IQueryable<Publication> ForAuthor(this IQueryable<Publication> query, int userId)
		=> query.Where(p => p.Authors.Select(pa => pa.UserId).Contains(userId));

	public static IQueryable<Publication> FilterByTokens(this IQueryable<Publication> publications, IPublicationTokens tokens)
	{
		if (tokens.MovieIds.Any())
		{
			return publications.Where(p => tokens.MovieIds.Contains(p.Id)).OrderBy(p => p.Id);
		}

		var query = publications;
		if (tokens.SystemCodes.Any())
		{
			query = query.Where(p => tokens.SystemCodes.Contains(p.System!.Code));
		}

		if (tokens.Games.Any())
		{
			query = query.Where(p => tokens.Games.Contains(p.GameId));
		}

		if (tokens.GameGroups.Any())
		{
			query = query.Where(p => p.Game!.GameGroups.Any(gg => tokens.GameGroups.Contains(gg.GameGroupId)));
		}

		if (tokens.Classes.Any())
		{
			query = query.Where(p => tokens.Classes.Contains(p.PublicationClass!.Name));
		}

		if (tokens.OnlyObsoleted)
		{
			query = query.ThatAreObsolete();
		}
		else if (!tokens.ShowObsoleted)
		{
			query = query.ThatAreCurrent();
		}

		if (tokens.Years.Any())
		{
			query = query.Where(p => tokens.Years.Contains(p.CreateTimestamp.Year));
		}

		if (tokens.Tags.Any())
		{
			query = query.Where(p => p.PublicationTags.Any(t => tokens.Tags.Contains(t.Tag!.Code)));
		}

		if (tokens.Genres.Any())
		{
			query = query.Where(p => p.Game!.GameGenres.Any(gg => tokens.Genres.Contains(gg.Genre!.DisplayName)));
		}

		if (tokens.Flags.Any())
		{
			query = query.Where(p => p.PublicationFlags.Any(f => tokens.Flags.Contains(f.Flag!.Token)));
		}

		if (tokens.Authors.Any())
		{
			query = query.Where(p => p.Authors.Select(a => a.UserId).Any(a => tokens.Authors.Contains(a)));
		}

		IOrderedQueryable<Publication> orderedQuery;
		if (!string.IsNullOrEmpty(tokens.SortBy))
		{
			orderedQuery = tokens.SortBy switch
			{
				"v" => query.OrderBy(p => p.CreateTimestamp),
				"u" => query.OrderByDescending(p => p.CreateTimestamp),
				"s" => query.OrderBy(p => p.Frames / (p.SystemFrameRate == null ? 60 : p.SystemFrameRate.FrameRate)),
				"l" => query.OrderByDescending(p => p.Frames / (p.SystemFrameRate == null ? 60 : p.SystemFrameRate.FrameRate)),
				_ => query
					.OrderBy(p => p.System!.Code)
					.ThenBy(p => p.Game!.DisplayName)
			};
		}
		else
		{
			orderedQuery = query
				.OrderBy(p => p.System!.Code)
				.ThenBy(p => p.Game!.DisplayName);
		}

		query = orderedQuery.ThenBy(p => p.Id);

		if (tokens.Limit.HasValue)
		{
			query = query.Take(tokens.Limit.Value);
		}

		return query;
	}

	public static IQueryable<Publication> IncludeTitleTables(this DbSet<Publication> query)
		=> query
			.Include(p => p.Authors)
			.ThenInclude(pa => pa.Author)
			.Include(p => p.System)
			.Include(p => p.SystemFrameRate)
			.Include(p => p.Game)
			.Include(p => p.GameVersion)
			.Include(p => p.GameGoal);
}
