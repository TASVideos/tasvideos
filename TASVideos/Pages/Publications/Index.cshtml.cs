using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Publications
{
	// TODO: add paging
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;
		private readonly IPointsCalculator _points;

		public IndexModel(
			ApplicationDbContext db,
			ICacheService cache,
			IPointsCalculator points)
		{
			_db = db;
			_cache = cache;
			_points = points;
		}

		[FromRoute]
		public string Query { get; set; }

		public IEnumerable<PublicationDisplayModel> Movies { get; set; } = new List<PublicationDisplayModel>();

		public async Task<IActionResult> OnGet()
		{
			var tokenLookup = await GetMovieTokenData();

			var tokens = Query
				.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim(' '))
				.Select(s => s.ToLower())
				.ToList();

			var searchModel = new PublicationSearchModel
			{
				Tiers = tokenLookup.Tiers.Where(t => tokens.Contains(t)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => tokens.Contains(s)),
				ShowObsoleted = tokens.Contains("obs"),
				Years = tokenLookup.Years.Where(y => tokens.Contains("y" + y)),
				Tags = tokenLookup.Tags.Where(t => tokens.Contains(t)),
				Genres = tokenLookup.Genres.Where(g => tokens.Contains(g)),
				Flags = tokenLookup.Flags.Where(f => tokens.Contains(f)),
				MovieIds = tokens
					.Where(t => t.EndsWith('m'))
					.Where(t => int.TryParse(t.Substring(0, t.Length - 1), out int unused))
					.Select(t => int.Parse(t.Substring(0, t.Length - 1)))
					.ToList(),
				Authors = tokens
					.Where(t => t.ToLower().Contains("author"))
					.Select(t => t.ToLower().Replace("author", ""))
					.Select(t => int.TryParse(t, out var temp) ? temp : (int?)null)
					.Where(t => t.HasValue)
					.Select(t => t.Value)
					.ToList()
			};

			// If no valid filter criteria, don't attempt to generate a list (else it would be all movies for what is most likely a malformed URL)
			if (searchModel.IsEmpty)
			{
				return Redirect("Movies");
			}

			Movies = await GetMovieList(searchModel);

			var ratings = (await _points.PublicationRatings(Movies.Select(m => m.Id)))
				.ToDictionary(tkey => tkey.Key, tvalue => tvalue.Value.Overall);

			foreach (var rating in ratings)
			{
				Movies.First(m => m.Id == rating.Key).OverallRating = rating.Value;
			}

			return Page();
		}

		private async Task<PublicationSearchModel> GetMovieTokenData()
		{
			if (_cache.TryGetValue(CacheKeys.MovieTokens, out PublicationSearchModel cachedResult))
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

				_cache.Set(CacheKeys.MovieTokens, result);

				return result;
			}
		}

		private async Task<IList<PublicationDisplayModel>> GetMovieList(PublicationSearchModel searchCriteria)
		{
			var query = _db.Publications
				.FilterByTokens(searchCriteria);

			// TODO: AutoMapper, single movie is the same logic
			return await query
				.OrderBy(p => p.System.Code)
				.ThenBy(p => p.Game.DisplayName)
				.Select(p => new PublicationDisplayModel
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
					Files = p.Files.Select(f => new PublicationDisplayModel.FileModel
					{
						Path = f.Path,
						Type = f.Type
					}).ToList(),
					Tags = p.PublicationTags
						.Select(pt => new PublicationDisplayModel.TagModel
						{
							DisplayName = pt.Tag.DisplayName,
							Code = pt.Tag.Code
						})
						.ToList(),
					GenreTags = p.Game.GameGenres
						.Select(gg => new PublicationDisplayModel.TagModel
						{
							DisplayName = gg.Genre.DisplayName,
							Code = gg.Genre.DisplayName // TODO
						})
						.ToList(),
					Flags = p.PublicationFlags
						.Where(pf => pf.Flag.IconPath != null)
						.Select(pf => new PublicationDisplayModel.FlagModel
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
