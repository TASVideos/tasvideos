using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
    public class HomePageHeader : ViewComponent
    {
		private readonly UserTasks _userTasks;

		public HomePageHeader(
			UserTasks userTasks,
			AwardTasks awardTasks)
		{
			_userTasks = userTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData)
		{
			//var model = "TODO";
			return View();
		}
	}
}
