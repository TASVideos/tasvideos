using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.UserMovies)]
public class UserMovies : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public UserMovies(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(int? limit)
	{
		var count = limit ?? 5;

		var userMovies = await _db.UserFiles
			.ThatAreMovies()
			.ThatArePublic()
			.ByRecentlyUploaded()
			.ToUserMovieListModel()
			.Take(count)
			.ToListAsync();

		return View(userMovies);
	}
}
