using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

// TODO: a better name for this is FrontPageMovie or something like that
[WikiModule(ModuleNames.DisplayMiniMovie)]
public class DisplayMiniMovie(ApplicationDbContext db) : WikiViewComponent
{
	public MiniMovieModel Movie { get; set; } = new MiniMovieModel { Title = "No Publications", OnlineWatchingUrl = "" };
	public async Task<IViewComponentResult> InvokeAsync(string? pubClass, IList<string> flags)
	{
		var query = db.Publications.ThatAreCurrent();

		if (!string.IsNullOrWhiteSpace(pubClass))
		{
			query = query.Where(p => p.PublicationClass!.Name == pubClass);
		}

		if (flags.Count > 0)
		{
			query = query.Where(p => p.PublicationFlags.Any(pf => flags.Contains(pf.Flag!.Token)));
		}

		var candidateIds = await query
			.Select(p => p.Id)
			.ToListAsync();

		var id = candidateIds.ToList().AtRandom();

		// id == 0 means there are no publications, which is an out-of-the-box problem only
		if (id != 0)
		{
			Movie = await db.Publications
				.ToMiniMovieModel()
				.SingleAsync(p => p.Id == id);
		}

		return View();
	}

	public class MiniMovieModel
	{
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public string Goal { get; init; } = "";
		public ScreenshotFile Screenshot { get; init; } = new();
		public string? OnlineWatchingUrl { get; init; }

		public class ScreenshotFile
		{
			public string Path { get; init; } = "";
			public string? Description { get; init; }
		}
	}
}
