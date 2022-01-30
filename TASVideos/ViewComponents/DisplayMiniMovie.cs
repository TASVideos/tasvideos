using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

// TODO: a better name for this is FrontPageMovie or something like that
[WikiModule(WikiModules.DisplayMiniMovie)]
public class DisplayMiniMovie : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public DisplayMiniMovie(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(string? tier, IList<string> flags)
	{
		var candidateIds = await FrontPageMovieCandidates(tier, flags);
		var id = candidateIds.ToList().AtRandom();
		var movie = await GetPublicationMiniMovie(id);
		return View(movie);
	}

	private async Task<IEnumerable<int>> FrontPageMovieCandidates(string? publicationClass, IList<string> flagsArr)
	{
		var query = _db.Publications
			.ThatAreCurrent()
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(publicationClass))
		{
			query = query.Where(p => p.PublicationClass!.Name == publicationClass);
		}

		if (flagsArr.Count > 0)
		{
			query = query.Where(p => p.PublicationFlags.Any(pf => flagsArr.Contains(pf.Flag!.Token)));
		}

		return await query
			.Select(p => p.Id)
			.ToListAsync();
	}

	private async Task<MiniMovieModel?> GetPublicationMiniMovie(int id)
	{
		// TODO: id == 0 means there are no publications, which is an out of the box problem only, make this scenario more clear and simpler
		if (id != 0)
		{
			return await _db.Publications
				.Select(p => new MiniMovieModel
				{
					Id = p.Id,
					Title = p.Title,
					Screenshot = p.Files
						.Where(f => f.Type == FileType.Screenshot)
						.Select(f => new MiniMovieModel.ScreenshotFile
						{
							Path = f.Path,
							Description = f.Description
						})
						.First(),
					OnlineWatchingUrl = p.PublicationUrls.First(u => u.Type == PublicationUrlType.Streaming).Url
				})
				.SingleOrDefaultAsync(p => p.Id == id);
		}

		return await _db.Publications
			.Select(p => new MiniMovieModel
			{
				Id = 0,
				Title = "Error",
				OnlineWatchingUrl = ""
			})
			.SingleOrDefaultAsync(p => p.Id == id);
	}
}
