using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;

namespace TASVideos.ViewComponents
{
	// TODO: a better name for this is FrontPageMovie or something like that
	public class DisplayMiniMovie : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public DisplayMiniMovie(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var tier = ParamHelper.GetValueFor(pp, "tier");
			var flags = ParamHelper.GetValueFor(pp, "flags");
			var candidateIds = await FrontPageMovieCandidates(tier, flags);
			var id = candidateIds.ToList().AtRandom();
			var movie = await GetPublicationMiniMovie(id);
			return View(movie);
		}

		private async Task<IEnumerable<int>> FrontPageMovieCandidates(string tier, string flags)
		{
			var query = _db.Publications
				.ThatAreCurrent()
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(tier))
			{
				query = query.Where(p => p.Tier.Name == tier);
			}

			if (!string.IsNullOrWhiteSpace(flags))
			{
				var flagsArr = flags.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				query = query.Where(p => p.PublicationFlags.Any(pf => flagsArr.Contains(pf.Flag.Token)));
			}

			return await query
				.Select(p => p.Id)
				.ToListAsync();
		}

		private async Task<MiniMovieModel> GetPublicationMiniMovie(int id)
		{
			// TODO: id == 0 means there are no publications, which is an out of the box problem only, make this scenario more clear and simpler
			if (id != 0)
			{
				return await _db.Publications
					.Select(p => new MiniMovieModel
					{
						Id = p.Id,
						Title = p.Title,
						Screenshot = p.Files.First(f => f.Type == FileType.Screenshot).Path,
						OnlineWatchingUrl = p.OnlineWatchingUrl,
					})
					.SingleOrDefaultAsync(p => p.Id == id);
			}

			return await _db.Publications
				.Select(p => new MiniMovieModel
				{
					Id = 0,
					Title = "Error",
					Screenshot = p.Files.FirstOrDefault(f => f.Type == FileType.Screenshot).Path,
					OnlineWatchingUrl = p.OnlineWatchingUrl,
				})
				.SingleOrDefaultAsync(p => p.Id == id);
		}
	}
}
