using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.ViewComponents
{
	public class UserMovies : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public UserMovies(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var count = ParamHelper.GetInt(pp, "limit").GetValueOrDefault(5);
			var userMovies = await _db.UserFiles
				.ThatAreMovies()
				.ThatArePublic()
				.ByRecentlyUploaded()
				.ProjectTo<UserMovieListModel>()
				.Take(count)
				.ToListAsync();

			// TODO
			var tier = ParamHelper.GetValueFor(pp, "tier");
			return View(userMovies);
		}
	}
}
