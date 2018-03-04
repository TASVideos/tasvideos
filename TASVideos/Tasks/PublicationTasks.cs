using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.ViewComponents;

namespace TASVideos.Tasks
{
	public class PublicationTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;
		
		public PublicationTasks(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		/// <summary>
		/// Gets all the possible values that can be tokens in the Movies- url
		/// </summary>
		public async Task<PublicationSearchModel> GetMovieTokenData()
		{
			var cacheKey = $"{nameof(PublicationTasks)}{nameof(GetMovieTokenData)}";
			if (_cache.IsSet(cacheKey))
			{
				return _cache.Get<PublicationSearchModel>(cacheKey);
			}

			var result = new PublicationSearchModel
			{
				Tiers = await _db.Tiers.Select(t => t.Name.ToLower()).ToListAsync(),
				SystemCodes = await _db.GameSystems.Select(s => s.Code.ToLower()).ToListAsync(),
				Tags = await _db.Tags.Select(t => t.Code.ToLower()).ToListAsync() // TODO: Game genres too?
			};

			_cache.Set(cacheKey, result);

			return result;
		}

		/// <summary>
		/// Gets a publication with the given <see cref="id" /> for the purpose of display
		/// If no publication with the given id is found then null is returned
		/// </summary>
		public async Task<PublicationViewModel> GetPublicationForDisplay(int id)
		{
			var publication = await _db.Publications
				.Include(p => p.Files)
				.Include(p => p.PublicationTags)
				.ThenInclude(pt => pt.Tag)
				.SingleOrDefaultAsync(p => p.Id == id);

			if (publication != null)
			{
				return new PublicationViewModel
				{
					Id = publication.Id,
					CreateTimeStamp = publication.CreateTimeStamp,
					Title = publication.Title,
					Screenshot = publication.Files.First(f => f.Type == FileType.Screenshot).Path,
					TorrentLink = publication.Files.First(f => f.Type == FileType.Torrent).Path,
					OnlineWatchingUrl = publication.OnlineWatchingUrl,
					MirrorSiteUrl = publication.MirrorSiteUrl,
					ObsoletedBy = publication.ObsoletedById,
					MovieFileName = publication.MovieFileName,
					SubmissionId = publication.SubmissionId,
					Tags = publication.PublicationTags.Select(pt => pt.Tag.DisplayName)
				};
			}

			return null;
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

		// TODO: documentation
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

			if (searchCriteria.Years.Any())
			{
				query = query.Where(p => searchCriteria.Years.Contains(p.CreateTimeStamp.Year));
			}

			if (searchCriteria.Tags.Any())
			{
				query = query.Where(p => p.PublicationTags.Any(t => searchCriteria.Tags.Contains(t.Tag.Code)));
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
					CreateTimeStamp = p.CreateTimeStamp,
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

		public async Task<IEnumerable<TabularMovieListResultModel>> GetTabularMovieList(TabularMovieListSearchModel searchCriteria)
		{
			var movies = await _db.Publications
				.Include(p => p.Tier)
				.Include(p => p.Game)
				.Include(p => p.System)
				.Include(p => p.SystemFrameRate)
				.Include(p => p.Authors)
				.ThenInclude(pa => pa.Author)
				.Where(p => searchCriteria.Tiers.Contains(p.Tier.Name))
				.OrderByDescending(p => p.CreateTimeStamp)
				.Take(searchCriteria.Limit)
				.ToListAsync();

			var results = movies
				.Select(m => new TabularMovieListResultModel
				{
					Id = m.Id,
					CreateTimeStamp = m.CreateTimeStamp,
					Time = m.Time,
					Game = m.Game.DisplayName,
					Authors = string.Join(",", m.Authors.Select(pa => pa.Author)),
					ObsoletedBy = null // TODO: previous logic
				})
				.ToList();

			return results;
		}
	}
}
