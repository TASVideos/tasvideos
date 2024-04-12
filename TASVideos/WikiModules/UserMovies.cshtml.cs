using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.UserMovies)]
public class UserMovies(ApplicationDbContext db) : WikiViewComponent
{
	public List<Pages.UserFiles.IndexModel.UserMovie> Movies { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? limit)
	{
		Movies = await db.UserFiles
			.ThatAreMovies()
			.ThatArePublic()
			.ByRecentlyUploaded()
			.ToUserMovieListModel()
			.Take(limit ?? 5)
			.ToListAsync();

		return View();
	}
}
