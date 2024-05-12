using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.DisplayMovies)]
public class DisplayMovies(ApplicationDbContext db, IMovieSearchTokens tokens) : WikiViewComponent
{
	public List<Pages.Publications.IndexModel.PublicationDisplay> Movies { get; set; } = [];
	public async Task<IViewComponentResult> InvokeAsync(
		IList<string> pubClass,
		IList<string> systemCode,
		bool obs,
		bool obsOnly,
		IList<int> year,
		IList<string> tag,
		IList<string> flag,
		IList<int> group,
		IList<int> id,
		IList<int> game,
		IList<int> author,
		string? sort,
		int? limit)
	{
		var tokenLookup = await tokens.GetTokens();

		var searchModel = new Pages.Publications.IndexModel.PublicationSearch
		{
			Classes = tokenLookup.Classes.Where(c => pubClass.Select(tt => tt.ToLower()).Contains(c)).ToList(),
			SystemCodes = tokenLookup.SystemCodes.Where(s => systemCode.Select(c => c.ToLower()).Contains(s)).ToList(),
			ShowObsoleted = obs,
			OnlyObsoleted = obsOnly,
			SortBy = sort?.ToLower() ?? "",
			Limit = limit,
			Years = tokenLookup.Years.Where(year.Contains).ToList(),
			Tags = tokenLookup.Tags.Where(t => tag.Select(tt => tt.ToLower()).Contains(t)).ToList(),
			Genres = tokenLookup.Genres.Where(g => tag.Select(tt => tt.ToLower()).Contains(g)).ToList(),
			Flags = tokenLookup.Flags.Where(f => flag.Select(ff => ff.ToLower()).Contains(f)).ToList(),
			MovieIds = id,
			Games = game,
			GameGroups = group,
			Authors = author
		};

		if (!searchModel.IsEmpty)
		{
			Movies = await db.Publications
				.FilterByTokens(searchModel)
				.ToViewModel(searchModel.SortBy == "y")
				.ToListAsync();
		}

		return View();
	}
}
