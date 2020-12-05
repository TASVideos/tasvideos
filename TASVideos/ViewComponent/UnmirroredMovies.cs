using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using static TASVideos.Data.Entity.PublicationUrlType;

namespace TASVideos.ViewComponents
{
	public class UnmirroredMovies : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public UnmirroredMovies(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(string pp)
		{
			var query = _db.Publications.AsQueryable();
				

			if (ParamHelper.HasParam(pp, "obs"))
			{
				query = query.Where(p => p.ObsoletedById.HasValue);
			}
			else if (ParamHelper.HasParam(pp, "current"))
			{
				query = query.Where(p => !p.ObsoletedById.HasValue);
			}

			if (ParamHelper.HasParam(pp, "allunstreamed"))
			{
				query = query.Where(p => p.PublicationUrls.All(u => u.Type != Streaming));
			}
			else if (ParamHelper.HasParam(pp, "unstreamed"))
			{
				query = query.Where(p => !p.PublicationUrls.Any(u => u.Type == Streaming || u.Type == Mirror));
			}
			
			else if (ParamHelper.HasParam(pp, "streamed"))
			{
				query = query.Where(p =>
					p.PublicationUrls.Any(u => u.Type == Streaming)
					&& p.PublicationUrls.All(u => u.Type != Mirror));
			}
			else if (ParamHelper.HasParam(pp, "noyoutube"))
			{
				query = query.Where(p =>
					p.PublicationUrls.Any(u => u.Type == Streaming)
					&& !p.PublicationUrls.Any(u => u.Type == Streaming && u.Url!.Contains("youtube")));
			}
			else if (ParamHelper.HasParam(pp, "playlist"))
			{
				query = query.Where(p => p.PublicationUrls.Any(u => u.Type == Streaming && u.Url!.Contains("view_play_list")));
			}
			else
			{
				query = query.Where(p => p.PublicationUrls.All(u => u.Type != Mirror));
			}

			var model = await query
				.OrderBy(p => p.SystemId)
				.Select(p => new UnmirroredMovieEntry
				{
					Id = p.Id,
					Title = p.Title,
					EncodePaths = p.Files
						.Where(f => f.Type == FileType.Torrent)
						.Select(f => f.Path)
						.ToList()
				})
				.ToListAsync();
				

			return View(model);
		}
	}
}
