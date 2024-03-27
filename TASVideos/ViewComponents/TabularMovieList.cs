using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.TabularMovieList)]
public class TabularMovieList(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(int? limit, IList<string> tier, string? flink, string? footer)
	{
		var search = new TabularMovieListSearchModel();
		if (limit.HasValue)
		{
			search.Limit = limit.Value;
		}

		if (tier.Count > 0)
		{
			search.PublicationClasses = tier;
		}

		ViewData["flink"] = flink;

		if (!string.IsNullOrWhiteSpace(footer))
		{
			footer = "More...";
		}

		ViewData["footer"] = footer;

		var model = await MovieList(search);

		return View(model);
	}

	private async Task<IEnumerable<TabularMovieListResultModel>> MovieList(TabularMovieListSearchModel searchCriteria)
	{
		var results = await db.Publications
			.Where(p => !searchCriteria.PublicationClasses.Any() || searchCriteria.PublicationClasses.Contains(p.PublicationClass!.Name))
			.ByMostRecent()
			.Take(searchCriteria.Limit)
			.Select(p => new TabularMovieListResultModel
			{
				Id = p.Id,
				CreateTimestamp = p.CreateTimestamp,
				Frames = p.Frames,
				FrameRate = p.SystemFrameRate!.FrameRate,
				System = p.System!.Code,
				Game = p.GameVersion != null && !string.IsNullOrEmpty(p.GameVersion.TitleOverride) ? p.GameVersion.TitleOverride : p.Game!.DisplayName,
				Goal = p.GameGoal!.DisplayName,
				Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				AdditionalAuthors = p.AdditionalAuthors,
				Screenshot = p.Files
					.Where(f => f.Type == FileType.Screenshot)
					.Select(f => new TabularMovieListResultModel.ScreenshotFile
					{
						Path = f.Path,
						Description = f.Description
					})
					.First(),
				ObsoletedMovie = p.ObsoletedMovies
					.Select(o => new TabularMovieListResultModel.ObsoletedPublication
					{
						Id = o.Id,
						Frames = o.Frames,
						FrameRate = o.SystemFrameRate!.FrameRate
					})
					.FirstOrDefault()
			})
			.ToListAsync();

		return results;
	}
}
