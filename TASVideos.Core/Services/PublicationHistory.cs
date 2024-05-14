namespace TASVideos.Core.Services.PublicationChain;

public interface IPublicationHistory
{
	/// <summary>
	/// Returns the publication history for a game,
	/// grouped by non-obsolete publications as the parent node
	/// </summary>
	Task<PublicationHistoryGroup?> ForGame(int gameId);

	/// <summary>
	/// Returns the publication history for a game associated with the given publication id.
	/// Note that this returns all publications for a game, not just the publication's chain
	/// </summary>
	Task<PublicationHistoryGroup?> ForGameByPublication(int publicationId);
}

internal class PublicationHistory(ApplicationDbContext db) : IPublicationHistory
{
	public async Task<PublicationHistoryGroup?> ForGame(int gameId)
	{
		var game = await db.Games.FindAsync(gameId);

		if (game is null)
		{
			return null;
		}

		var publications = await db.Publications
			.Where(p => p.GameId == gameId)
			.Select(p => new PublicationHistoryNode
			{
				Id = p.Id,
				Title = p.Title,
				Goal = p.GameGoal!.DisplayName,
				CreateTimestamp = p.CreateTimestamp,
				ObsoletedById = p.ObsoletedById,
				ClassIconPath = p.PublicationClass!.IconPath,
				Flags = p.PublicationFlags
					.Select(pf => new PublicationHistoryNode.FlagEntry(
						pf.Flag!.IconPath, pf.Flag!.LinkPath, pf.Flag!.Name))
			})
			.ToListAsync();

		// TODO: this is an n squared problem. Any way to avoid it?
		// Realistically, no publication history is going to be large enough to cause a major problem
		foreach (var pub in publications)
		{
			pub.ObsoleteList = publications.Where(p => p.ObsoletedById == pub.Id).ToList();
		}

		return new PublicationHistoryGroup
		{
			GameId = gameId,
			GameDisplayName = game.DisplayName,
			Goals = publications
				.Where(p => !p.ObsoletedById.HasValue)
				.ToList()
		};
	}

	public async Task<PublicationHistoryGroup?> ForGameByPublication(int publicationId)
	{
		var pub = await db.Publications
			.Select(p => new { p.Id, p.GameId })
			.SingleOrDefaultAsync(p => p.Id == publicationId);

		if (pub is null)
		{
			return null;
		}

		return await ForGame(pub.GameId);
	}
}
