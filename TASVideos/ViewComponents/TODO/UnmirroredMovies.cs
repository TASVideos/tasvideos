using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.WikiEngine;
using static TASVideos.Data.Entity.PublicationUrlType;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.UnmirroredMovies)]
public class UnmirroredMovies(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(
		bool obs,
		bool current,
		bool allUnstreamed,
		bool unstreamed,
		bool streamed,
		bool noYoutube,
		bool playlist)
	{
		var query = db.Publications.AsQueryable();

		if (obs)
		{
			query = query.Where(p => p.ObsoletedById.HasValue);
		}
		else if (current)
		{
			query = query.Where(p => !p.ObsoletedById.HasValue);
		}

		if (allUnstreamed)
		{
			query = query.Where(p => p.PublicationUrls.All(u => u.Type != Streaming));
		}
		else if (unstreamed)
		{
			query = query.Where(p => !p.PublicationUrls.Any(u => u.Type == Streaming || u.Type == Mirror));
		}
		else if (streamed)
		{
			query = query.Where(p =>
				p.PublicationUrls.Any(u => u.Type == Streaming)
				&& p.PublicationUrls.All(u => u.Type != Mirror));
		}
		else if (noYoutube)
		{
			query = query.Where(p =>
				p.PublicationUrls.Any(u => u.Type == Streaming)
				&& !p.PublicationUrls.Any(u => u.Type == Streaming && u.Url!.Contains("youtube")));
		}
		else if (playlist)
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
				Title = p.Title
			})
			.ToListAsync();

		return View(model);
	}
}
