using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
    public class HomePageFooter : ViewComponent
    {
		private readonly UserTasks _userTasks;

		public HomePageFooter(UserTasks userTasks)
		{
			_userTasks = userTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData)
		{
			var model = await _userTasks.GetUserSummary(pageData.PageName.Replace("HomePages/", ""));
			ViewData["pageData"] = pageData;
			return View(model);
		}
	}
}
