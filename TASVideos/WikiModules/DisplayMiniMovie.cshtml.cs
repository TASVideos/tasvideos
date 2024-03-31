using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

// TODO: a better name for this is FrontPageMovie or something like that
[WikiModule(ModuleNames.DisplayMiniMovie)]
public class DisplayMiniMovie(ApplicationDbContext db) : WikiViewComponent
{
	public MiniMovieModel? Movie { get; set; }
	public async Task<IViewComponentResult> InvokeAsync(string? pubClass, IList<string> flags)
	{
		var candidateIds = await FrontPageMovieCandidates(pubClass, flags);
		var id = candidateIds.ToList().AtRandom();
		Movie = await GetPublicationMiniMovie(id);
		return View();
	}

	private async Task<IEnumerable<int>> FrontPageMovieCandidates(string? publicationClass, IList<string> flagsArr)
	{
		var query = db.Publications
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
		// TODO: id == 0 means there are no publications, which is an out-of-the-box problem only, make this scenario more clear and simpler
		if (id != 0)
		{
			return await db.Publications
				.ToMiniMovieModel()
				.SingleOrDefaultAsync(p => p.Id == id);
		}

		return await db.Publications
			.Select(p => new MiniMovieModel
			{
				Id = 0,
				Title = "Error",
				OnlineWatchingUrl = ""
			})
			.SingleOrDefaultAsync(p => p.Id == id);
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
