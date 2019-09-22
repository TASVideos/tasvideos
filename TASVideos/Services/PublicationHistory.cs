using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;

namespace TASVideos.Services.PublicationChain
{
	public interface IPublicationHistory
	{
		/// <summary>
		/// Returns the publication history for a game,
		/// grouped by non-obsolete publications as the parent node
		/// </summary>
		Task<PublicationHistoryGroup> ForGame(int gameId);
	}

	public class PublicationHistory : IPublicationHistory
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public PublicationHistory(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		public async Task<PublicationHistoryGroup> ForGame(int gameId)
		{
			var game = await _db.Games
				.SingleOrDefaultAsync(g => g.Id == gameId);

			if (game == null)
			{
				return null;
			}

			var parents = new List<PublicationHistoryNode>();

			var publications = await _db.Publications
				.Where(p => p.GameId == gameId)
				.Select(p => new
				{
					p.Id,
					p.Title,
					p.Branch,
					p.ObsoletedById
				})
				.ToListAsync();

			var parentNodes = publications
				.Where(p => !p.ObsoletedById.HasValue)
				.ToList();

			foreach (var node in parentNodes)
			{
				parents.Add(new PublicationHistoryNode
				{
					Id = node.Id,
					Title = node.Title,
					Branch = node.Branch
				});
			}

			return new PublicationHistoryGroup
			{
				GameId = gameId,
				Branches = parents
			};
		}
	}
}
