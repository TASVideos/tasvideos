using TASVideos.Common;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.TabularMovieList)]
public class TabularMovieList(ApplicationDbContext db) : WikiViewComponent
{
	public List<TabularMovieEntry> Movies { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? limit, IList<string> tier)
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

		Movies = await MovieList(search);

		return View();
	}

	private async Task<List<TabularMovieEntry>> MovieList(TabularMovieListSearchModel searchCriteria)
	{
		var results = await db.Publications
			.Where(p => !searchCriteria.PublicationClasses.Any() || searchCriteria.PublicationClasses.Contains(p.PublicationClass!.Name))
			.ByMostRecent()
			.Take(searchCriteria.Limit)
			.Select(p => new TabularMovieEntry
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
					.Select(f => new TabularMovieEntry.ScreenshotFile
					{
						Path = f.Path,
						Description = f.Description
					})
					.First(),
				ObsoletedMovie = p.ObsoletedMovies
					.Select(o => new TabularMovieEntry.ObsoletedPublication
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

	public class TabularMovieListSearchModel
	{
		public int Limit { get; set; } = 10;
		public IEnumerable<string> PublicationClasses { get; set; } = [];
	}

	public class TabularMovieEntry : ITimeable
	{
		public int Id { get; set; }
		public DateTime CreateTimestamp { get; set; }

		public string System { get; set; } = "";
		public string Game { get; set; } = "";
		public string? Goal { get; set; }
		public IEnumerable<string>? Authors { get; set; }
		public string? AdditionalAuthors { get; set; }

		public ScreenshotFile Screenshot { get; set; } = new();

		public class ScreenshotFile
		{
			public string Path { get; set; } = "";
			public string? Description { get; set; }
		}

		public int Frames { get; set; }
		public double FrameRate { get; set; }

		public ObsoletedPublication? ObsoletedMovie { get; set; }

		public class ObsoletedPublication : ITimeable
		{
			public int Id { get; set; }
			public int Frames { get; set; }
			public double FrameRate { get; set; }
		}
	}
}
