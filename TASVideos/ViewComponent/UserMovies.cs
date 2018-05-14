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
			var userMovies = await _userFileTasks.GetLatest(5);
			return View(userMovies);
		}
	}
}
