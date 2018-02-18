using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Tasks
{
	public class PublicationTasks
	{
		private readonly ApplicationDbContext _db;

		public PublicationTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Gets all the possible values that can be tokens in the Movies- url
		/// </summary>
		public async Task<PublicationSearchModel> GetMovieTokenData()
		{
			// TODO: cache this call
			return new PublicationSearchModel
			{
				Tiers = await _db.Tiers.Select(t => t.Name).ToListAsync(),
				SystemCodes = await _db.GameSystems.Select(s => s.Code).ToListAsync()
			};
		}

		/// <summary>
		/// Gets a publication with the given <see cref="id" /> for the purpose of display
		/// If no publication with the given id is found then null is returned
		/// </summary>
		public async Task<PublicationViewModel> GetPublicationForDisplay(int id)
		{
			return await _db.Publications
				.Select(p => new PublicationViewModel
				{
					Id = p.Id,
					Title = p.Title,
					Screenshot = p.Files.First(f => f.Type == FileType.Screenshot).Path,
					TorrentLink = p.Files.First(f => f.Type == FileType.Torrent).Path,
					OnlineWatchingUrl = p.OnlineWatchingUrl,
					MirrorSiteUrl = p.MirrorSiteUrl,
					ObsoletedBy = p.ObsoletedById,
					MovieFileName = p.MovieFileName,
					SubmissionId = p.SubmissionId
				})
				.SingleOrDefaultAsync(p => p.Id == id);
		}

		/// <summary>
		/// Gets publication data for the DisplayMiniMovie module
		/// </summary>
		public async Task<MiniMovieModel> GetPublicationMiniMovie(int id)
		{
			return await _db.Publications
				.Select(p => new MiniMovieModel
				{
					Id = p.Id,
					Title = p.Title,
					Screenshot = p.Files.First(f => f.Type == FileType.Screenshot).Path,
					OnlineWatchingUrl = p.OnlineWatchingUrl,
				})
				.SingleAsync(p => p.Id == id);
		}

		/// <summary>
		/// Gets the title of a movie with the given id
		/// If the movie is not found, null is returned
		/// </summary>
		public async Task<string> GetTitle(int id)
		{
			return (await _db.Publications.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}

		/// <summary>
		/// Returns the publication file as bytes with the given id
		/// If no publication is found, an empty byte array is returned
		/// </summary>
		public async Task<(byte[], string)> GetPublicationMovieFile(int id)
		{
			var data = await _db.Publications
				.Where(s => s.Id == id)
				.Select(s => new { s.MovieFile, s.MovieFileName })
				.SingleOrDefaultAsync();

			if (data == null)
			{
				return (new byte[0], "");
			}

			return (data.MovieFile, data.MovieFileName);
		}

		// TODO: paging
		public async Task<IEnumerable<PublicationViewModel>> GetMovieList(PublicationSearchModel searchCriteria)
		{
			var query = _db.Publications
				.Include(p => p.Game)
				.Include(p => p.Files)
				.Include(p => p.Tier)
				.Include(p => p.System)
				.AsQueryable();

			if (searchCriteria.SystemCodes.Any())
			{
				query = query.Where(p => searchCriteria.SystemCodes.Contains(p.System.Code));
			}

			if (searchCriteria.Tiers.Any())
			{
				query = query.Where(p => searchCriteria.Tiers.Contains(p.Tier.Name));
			}

			if (!searchCriteria.ShowObsoleted)
			{
				query = query.ThatAreCurrent();
			}

			var results = await query
				.OrderBy(p => p.System.Code)
				.ThenBy(p => p.Game.DisplayName)
				.ToListAsync();

			// TODO: automapper, single movie is the same logic
			return results
				.Select(p => new PublicationViewModel
				{
					Id = p.Id,
					Title = p.Title,
					Screenshot = p.Files.First(f => f.Type == FileType.Screenshot).Path,
					TorrentLink = p.Files.First(f => f.Type == FileType.Torrent).Path,
					OnlineWatchingUrl = p.OnlineWatchingUrl,
					MirrorSiteUrl = p.MirrorSiteUrl,
					ObsoletedBy = p.ObsoletedById,
					MovieFileName = p.MovieFileName,
					SubmissionId = p.SubmissionId
				})
				.ToList();
		}
	}
}
