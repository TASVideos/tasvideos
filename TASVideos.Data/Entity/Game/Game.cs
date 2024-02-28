﻿namespace TASVideos.Data.Entity.Game;

/// <summary>
/// Represents a Game
/// This is the central reference point for all site content
/// </summary>
public class Game : BaseEntity
{
	public int Id { get; set; }
	public virtual ICollection<GameVersion> GameVersions { get; set; } = new HashSet<GameVersion>();

	public virtual ICollection<Publication> Publications { get; set; } = new HashSet<Publication>();
	public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
	public virtual ICollection<GameGenre> GameGenres { get; set; } = new HashSet<GameGenre>();
	public virtual ICollection<UserFile> UserFiles { get; set; } = new HashSet<UserFile>();
	public virtual ICollection<GameGameGroup> GameGroups { get; set; } = new HashSet<GameGameGroup>();
	public virtual ICollection<GameGoal> GameGoals { get; set; } = new HashSet<GameGoal>();

	[StringLength(100)]
	public string DisplayName { get; set; } = "";

	[StringLength(24)]
	public string? Abbreviation { get; set; }

	[StringLength(250)]
	public string? Aliases { get; set; }

	[StringLength(250)]
	public string? ScreenshotUrl { get; set; }

	[StringLength(300)]
	public string? GameResourcesPage { get; set; }
}

public static class GameExtensions
{
	public static IQueryable<Game> ForGroup(this IQueryable<Game> query, int gameGroupId)
	{
		return query.Where(g => g.GameGroups.Any(gg => gg.GameGroupId == gameGroupId));
	}

	public static IQueryable<Game> ForSystem(this IQueryable<Game> query, int systemId)
	{
		return query.Where(g => g.GameVersions.Count == 0 || g.GameVersions.Any(r => r.SystemId == systemId));
	}

	public static IQueryable<Game> ForSystemCode(this IQueryable<Game> query, string? code)
	{
		return !string.IsNullOrWhiteSpace(code)
			? query.Where(g => g.GameVersions.Count == 0 || g.GameVersions.Any(r => r.System!.Code == code))
			: query;
	}

	public static IQueryable<Game> ForSystemCodes(this IQueryable<Game> query, IEnumerable<string> codes)
	{
		var codeList = codes.ToList();
		return codeList.Any()
			? query.Where(g => g.GameVersions.Select(r => r.System!.Code).Any(c => codeList.Contains(c)))
			: query;
	}

	public static IQueryable<Game> ForGenre(this IQueryable<Game> query, string? genre)
	{
		return !string.IsNullOrWhiteSpace(genre)
			? query.Where(g => g.GameGenres.Any(gg => gg.Genre!.DisplayName == genre))
			: query;
	}

	public static IQueryable<Game> ForGroup(this IQueryable<Game> query, string? group)
	{
		return !string.IsNullOrWhiteSpace(group)
			? query.Where(g => g.GameGroups.Any(gg => gg.GameGroup!.Name == group))
			: query;
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
