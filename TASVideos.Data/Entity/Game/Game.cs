using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity.Game;

/// <summary>
/// Represents a Game
/// This is the central reference point for all site content
/// </summary>
[IncludeInAutoHistory]
public class Game : BaseEntity
{
	public int Id { get; set; }
	public ICollection<GameVersion> GameVersions { get; init; } = [];

	public ICollection<Publication> Publications { get; init; } = [];
	public ICollection<Submission> Submissions { get; init; } = [];
	public ICollection<GameGenre> GameGenres { get; init; } = [];
	public ICollection<UserFile> UserFiles { get; init; } = [];
	public ICollection<GameGameGroup> GameGroups { get; init; } = [];
	public ICollection<GameGoal> GameGoals { get; init; } = [];

	public string DisplayName { get; set; } = "";

	public string? Abbreviation { get; set; }

	public string? Aliases { get; set; }

	public string? ScreenshotUrl { get; set; }

	public string? GameResourcesPage { get; set; }
}

public static class GameExtensions
{
	public static IQueryable<Game> ForGroup(this IQueryable<Game> query, int gameGroupId)
		=> query.Where(g => g.GameGroups.Any(gg => gg.GameGroupId == gameGroupId));

	public static IQueryable<Game> ForSystem(this IQueryable<Game> query, int systemId)
		=> query.Where(g => g.GameVersions.Count == 0 || g.GameVersions.Any(r => r.SystemId == systemId));

	public static IQueryable<Game> ForSystemCode(this IQueryable<Game> query, string? code)
		=> !string.IsNullOrWhiteSpace(code)
			? query.Where(g => g.GameVersions.Count == 0 || g.GameVersions.Any(r => r.System!.Code == code))
			: query;

	public static IQueryable<Game> ForSystemCodes(this IQueryable<Game> query, ICollection<string> codes)
	{
		return codes.Any()
			? query.Where(g => g.GameVersions.Select(r => r.System!.Code).Any(c => codes.Contains(c)))
			: query;
	}

	public static IQueryable<Game> ForGenre(this IQueryable<Game> query, string? genre)
		=> !string.IsNullOrWhiteSpace(genre)
			? query.Where(g => g.GameGenres.Any(gg => gg.Genre!.DisplayName == genre))
			: query;

	public static IQueryable<Game> ForGroup(this IQueryable<Game> query, string? group)
		=> !string.IsNullOrWhiteSpace(group)
			? query.Where(g => g.GameGroups.Any(gg => gg.GameGroup!.Name == group))
			: query;

	public static IQueryable<Game> WebSearch(this IQueryable<Game> query, string searchTerms)
		=> query.Where(g => EF.Functions.ToTsVector("simple", g.DisplayName.Replace("/", " ") + " || " + g.Aliases + " || " + g.Abbreviation).Matches(EF.Functions.WebSearchToTsQuery("simple", searchTerms)));

	public static IOrderedQueryable<Game> ByWebRanking(this IQueryable<Game> query, string searchTerms)
		=> query.OrderByDescending(g => EF.Functions.ToTsVector("simple", g.DisplayName.Replace("/", " ")).ToStripped().Rank(EF.Functions.WebSearchToTsQuery("simple", searchTerms), NpgsqlTsRankingNormalization.DivideByLength));

	public static void SetGenres(this ICollection<GameGenre> genres, IEnumerable<int> genreIds)
	{
		genres.Clear();
		genres.AddRange(genreIds.Select(g => new GameGenre
		{
			GenreId = g
		}));
	}

	public static void SetGroups(this ICollection<GameGameGroup> groups, IEnumerable<int> groupIds)
	{
		groups.Clear();
		groups.AddRange(groupIds.Select(g => new GameGameGroup
		{
			GameGroupId = g
		}));
	}
}
