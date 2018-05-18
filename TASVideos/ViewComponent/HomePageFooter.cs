using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
    public class HomePageFooter : ViewComponent
    {
		private readonly UserTasks _userTasks;
		private readonly AwardTasks _awardTasks;

		public HomePageFooter(
			UserTasks userTasks,
			AwardTasks awardTasks)
		{
			_userTasks = userTasks;
			_awardTasks = awardTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData)
		{
			var model = await _userTasks.GetUserSummary(pageData.PageName.Replace("HomePages/", ""));
			model.AwardsWon = (await _awardTasks.GetAllAwardsForUser(model.Id)).Count();
			ViewData["pageData"] = pageData;
			return View(model);
		}
	}
}
