using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.UserMovies)]
public class UserMovies(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(int? limit)
	{
		var count = limit ?? 5;

		var userMovies = await db.UserFiles
			.ThatAreMovies()
			.ThatArePublic()
			.ByRecentlyUploaded()
			.ToUserMovieListModel()
			.Take(count)
			.ToListAsync();

		return View(userMovies);
	}
}
