using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

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
			if (_cache.TryGetValue(cacheKey, out PublicationSearchModel cachedResult))
			{
				return cachedResult;
			}

			using (await _db.Database.BeginTransactionAsync())
			{
				var result = new PublicationSearchModel
				{
					Tiers = await _db.Tiers.Select(t => t.Name.ToLower()).ToListAsync(),
					SystemCodes = await _db.GameSystems.Select(s => s.Code.ToLower()).ToListAsync(),
					Tags = await _db.Tags.Select(t => t.Code.ToLower()).ToListAsync(),
					Genres = await _db.Genres.Select(g => g.DisplayName.ToLower()).ToListAsync(),
					Flags = await _db.Flags.Select(f => f.Token.ToLower()).ToListAsync()
				};

				_cache.Set(cacheKey, result);

				return result;
			}
		}

		/// <summary>
		/// Gets the title of a movie with the given id
		/// If the movie is not found, null is returned
		/// </summary>
		public async Task<string> GetTitle(int id)
		{
			return (await _db.Publications
				.Select(s => new { s.Id, s.Title })
				.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}

		// TODO: paging
		/// <summary>
		/// Returns a list of publications with the given <see cref="searchCriteria" />
		/// for the purpose of displaying on a movie listings page
		/// </summary>
		public async Task<IEnumerable<PublicationModel>> GetMovieList(PublicationSearchModel searchCriteria)
		{
			var query = _db.Publications
				.AsQueryable();

			if (searchCriteria.MovieIds.Any())
			{
				query = query.Where(p => searchCriteria.MovieIds.Contains(p.Id));
			}
			else
			{
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

				if (searchCriteria.Genres.Any())
				{
					query = query.Where(p => p.Game.GameGenres.Any(gg => searchCriteria.Genres.Contains(gg.Genre.DisplayName)));
				}

				if (searchCriteria.Flags.Any())
				{
					query = query.Where(p => p.PublicationFlags.Any(f => searchCriteria.Flags.Contains(f.Flag.Token)));
				}

				if (searchCriteria.Authors.Any())
				{
					query = query.Where(p => p.Authors.Select(a => a.UserId).Any(a => searchCriteria.Authors.Contains(a)));
				}
			}

			// TODO: AutoMapper, single movie is the same logic
			return await query
				.OrderBy(p => p.System.Code)
				.ThenBy(p => p.Game.DisplayName)
				.Select(p => new PublicationModel
				{
					Id = p.Id,
					CreateTimeStamp = p.CreateTimeStamp,
					Title = p.Title,
					OnlineWatchingUrl = p.OnlineWatchingUrl,
					MirrorSiteUrl = p.MirrorSiteUrl,
					ObsoletedBy = p.ObsoletedById,
					MovieFileName = p.MovieFileName,
					SubmissionId = p.SubmissionId,
					RatingCount = p.PublicationRatings.Count / 2,
					TierIconPath = p.Tier.IconPath,
					Files = p.Files.Select(f => new PublicationModel.FileModel
					{
						Path = f.Path,
						Type = f.Type
					}).ToList(),
					Tags = p.PublicationTags
						.Select(pt => new PublicationModel.TagModel
						{
							DisplayName = pt.Tag.DisplayName,
							Code = pt.Tag.Code
						})
						.ToList(),
					GenreTags = p.Game.GameGenres
						.Select(gg => new PublicationModel.TagModel
						{
							DisplayName = gg.Genre.DisplayName,
							Code = gg.Genre.DisplayName // TODO
						})
						.ToList(),
					Flags = p.PublicationFlags
						.Where(pf => pf.Flag.IconPath != null)
						.Select(pf => new PublicationModel.FlagModel
						{
							IconPath = pf.Flag.IconPath,
							LinkPath = pf.Flag.LinkPath,
							Name = pf.Flag.Name
						})
						.ToList()
				})
				.ToListAsync();
		}
	}
}
