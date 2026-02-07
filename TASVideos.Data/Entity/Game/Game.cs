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
	extension(IQueryable<Game> query)
	{
		public IQueryable<Game> ForGroup(int gameGroupId)
			=> query.Where(g => g.GameGroups.Any(gg => gg.GameGroupId == gameGroupId));

		public IQueryable<Game> ForSystem(int systemId)
			=> query.Where(g => g.GameVersions.Count == 0 || g.GameVersions.Any(r => r.SystemId == systemId));

		public IQueryable<Game> ForSystemCode(string? code)
			=> !string.IsNullOrWhiteSpace(code)
				? query.Where(g => g.GameVersions.Count == 0 || g.GameVersions.Any(r => r.System!.Code == code))
				: query;

		public IQueryable<Game> ForSystemCodes(ICollection<string> codes)
			=> codes.Any()
				? query.Where(g => g.GameVersions.Select(r => r.System!.Code).Any(c => codes.Contains(c)))
				: query;

		public IQueryable<Game> ForGenre(string? genre)
			=> !string.IsNullOrWhiteSpace(genre)
				? query.Where(g => g.GameGenres.Any(gg => gg.Genre!.DisplayName == genre))
				: query;

		public IQueryable<Game> ForGroup(string? group)
			=> !string.IsNullOrWhiteSpace(group)
				? query.Where(g => g.GameGroups.Any(gg => gg.GameGroup!.Name == group))
				: query;

		public IQueryable<Game> WebSearch(string searchTerms)
			=> query.Where(g => EF.Functions.ToTsVector("simple", g.DisplayName.Replace("/", " ") + " || " + g.Aliases + " || " + g.Abbreviation).Matches(EF.Functions.WebSearchToTsQuery("simple", searchTerms)));

		public IOrderedQueryable<Game> ByWebRanking(string searchTerms)
			=> query.OrderByDescending(g => EF.Functions.ToTsVector("simple", g.DisplayName.Replace("/", " ")).ToStripped().Rank(EF.Functions.WebSearchToTsQuery("simple", searchTerms), NpgsqlTsRankingNormalization.DivideByLength));
	}

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
