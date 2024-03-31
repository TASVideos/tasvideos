using TASVideos.Pages.UserFiles.Models;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.UserMovies)]
public class UserMovies(ApplicationDbContext db) : WikiViewComponent
{
	public List<UserMovieListModel> Movies { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? limit)
	{
		var count = limit ?? 5;

		Movies = await db.UserFiles
			.ThatAreMovies()
			.ThatArePublic()
			.ByRecentlyUploaded()
			.ToUserMovieListModel()
			.Take(count)
			.ToListAsync();

		return View();
	}
}
