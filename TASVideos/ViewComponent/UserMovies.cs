using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class UserMovies : ViewComponent
    {
		private readonly UserFileTasks _userFileTasks;

		public UserMovies(UserFileTasks userFileTasks)
		{
			_userFileTasks = userFileTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var count = ParamHelper.GetInt(pp, "limit").GetValueOrDefault(5);
			var userMovies = await _userFileTasks.GetLatest(count);

			// TODO
			var tier = ParamHelper.GetValueFor(pp, "tier");
			return View(userMovies);
		}
	}
}
