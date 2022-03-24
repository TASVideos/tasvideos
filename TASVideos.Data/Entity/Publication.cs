using System.ComponentModel.DataAnnotations.Schema;
using TASVideos.Common;
using TASVideos.Data.Entity.Awards;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.Entity;

/// <summary>
/// Represents filter criteria for filtering publications.
/// </summary>
public interface IPublicationTokens
{
	IEnumerable<string> SystemCodes { get; }
	IEnumerable<string> Classes { get; }
	IEnumerable<int> Years { get; }
	IEnumerable<string> Tags { get; }
	IEnumerable<string> Genres { get; }
	IEnumerable<string> Flags { get; }
	IEnumerable<int> Authors { get; }
	IEnumerable<int> MovieIds { get; }
	IEnumerable<int> Games { get; }
	IEnumerable<int> GameGroups { get; }
	bool ShowObsoleted { get; }
	bool OnlyObsoleted { get; }
	string SortBy { get; }
	int? Limit { get; }
}

public class Publication : BaseEntity, ITimeable
{
	public int Id { get; set; }

	public virtual ICollection<PublicationFile> Files { get; set; } = new HashSet<PublicationFile>();
	public virtual ICollection<PublicationTag> PublicationTags { get; set; } = new HashSet<PublicationTag>();
	public virtual ICollection<PublicationFlag> PublicationFlags { get; set; } = new HashSet<PublicationFlag>();
	public virtual ICollection<PublicationAward> PublicationAwards { get; set; } = new HashSet<PublicationAward>();
	public virtual ICollection<PublicationMaintenanceLog> PublicationMaintenanceLogs { get; set; } = new HashSet<PublicationMaintenanceLog>();

	[ForeignKey(nameof(PublicationRating.PublicationId))]
	public virtual ICollection<PublicationRating> PublicationRatings { get; set; } = new HashSet<PublicationRating>();

	public virtual ICollection<PublicationUrl> PublicationUrls { get; set; } = new HashSet<PublicationUrl>();

	public int? ObsoletedById { get; set; }
	public virtual Publication? ObsoletedBy { get; set; }

	public virtual ICollection<Publication> ObsoletedMovies { get; set; } = new HashSet<Publication>();

	public int GameId { get; set; }
	public virtual Game.Game? Game { get; set; }

	public int SystemId { get; set; }
	public virtual GameSystem? System { get; set; }

	public int SystemFrameRateId { get; set; }
	public virtual GameSystemFrameRate? SystemFrameRate { get; set; }

	public int RomId { get; set; }
	public virtual GameRom? Rom { get; set; }

	public int PublicationClassId { get; set; }
	public virtual PublicationClass? PublicationClass { get; set; }

	public int SubmissionId { get; set; }
	public virtual Submission? Submission { get; set; }
	public virtual ICollection<PublicationAuthor> Authors { get; set; } = new HashSet<PublicationAuthor>();

	public int? WikiContentId { get; set; } // making this non-nullable is a catch-22 when creating a publication, the wiki needs a publication id and the publication needs a wiki id
	public virtual WikiPage? WikiContent { get; set; }

	[Required]
	public byte[] MovieFile { get; set; } = Array.Empty<byte>();

	[Required]
	public string MovieFileName { get; set; } = "";

	[StringLength(50)]
	public string? Branch { get; set; }

	[StringLength(50)]
	public string? EmulatorVersion { get; set; }

	public int Frames { get; set; }
	public int RerecordCount { get; set; }

	/// <summary>
	/// Gets or sets Any author's that are not a user. If they are a user, they should linked, and not listed here.
	/// </summary>
	[StringLength(200)]
	public string? AdditionalAuthors { get; set; }

	// De-normalized name for easy recreation
	[Required]
	public string Title { get; set; } = "";

	double ITimeable.FrameRate => SystemFrameRate?.FrameRate ?? throw new InvalidOperationException($"{nameof(SystemFrameRate)} must not be lazy loaded!");

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

		Title =
			$"{System.Code} {Game.DisplayName}"
			+ (!string.IsNullOrWhiteSpace(Branch) ? $" \"{Branch}\"" : "")
			+ $" by {string.Join(", ", authorList).LastCommaToAmpersand()}"
			+ $" in {this.Time().ToStringWithOptionalDaysAndHours()}";
	}
}

public static class PublicationExtensions
{
	public static IQueryable<Publication> ThatAreCurrent(this IQueryable<Publication> publications)
	{
		return publications.Where(p => p.ObsoletedById == null);
	}

	public static IEnumerable<Publication> ThatAreCurrent(this IEnumerable<Publication> publications)
	{
		return publications.Where(p => p.ObsoletedById == null);
	}

	public static IQueryable<Publication> ThatAreObsolete(this IQueryable<Publication> publications)
	{
		return publications.Where(p => p.ObsoletedById != null);
	}

	public static IQueryable<Publication> ForYearRange(this IQueryable<Publication> publications, int before, int after)
	{
		return publications
			.Where(p => p.CreateTimestamp.Year < before)
			.Where(p => p.CreateTimestamp.Year >= after);
	}

	public static IQueryable<Publication> ForDateRange(this IQueryable<Publication> publications, DateTime before, DateTime after)
	{
		return publications
			.Where(p => p.CreateTimestamp < before)
			.Where(p => p.CreateTimestamp >= after);
	}

	public static IQueryable<Publication> ThatHaveBeenPublishedBy(this IQueryable<Publication> publications, int userId)
	{
		return publications.Where(p => p.Submission!.PublisherId == userId);
	}

	public static IQueryable<Publication> ForAuthor(this IQueryable<Publication> publications, int userId)
	{
		return publications.Where(p => p.Authors.Select(pa => pa.UserId).Contains(userId));
	}

	public static IQueryable<Publication> FilterByTokens(this IQueryable<Publication> publications, IPublicationTokens tokens)
	{
		if (tokens.MovieIds.Any())
		{
			return publications.Where(p => tokens.MovieIds.Contains(p.Id));
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

		if (!string.IsNullOrEmpty(tokens.SortBy))
		{
			query = tokens.SortBy switch
			{
				"v" => query.OrderBy(p => p.CreateTimestamp),
				"u" => query.OrderByDescending(p => p.CreateTimestamp),
				_ => query
			};
		}
		else
		{
			query = query
				.OrderBy(p => p.System!.Code)
				.ThenBy(p => p.Game!.DisplayName);
		}

		if (tokens.Limit.HasValue)
		{
			query = query.Take(tokens.Limit.Value);
		}

		return query;
	}
}
