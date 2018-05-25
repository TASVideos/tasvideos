using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class Awards : ViewComponent
	{
		private readonly AwardTasks _awardTasks;

		public Awards(AwardTasks awardTasks)
		{
			_awardTasks = awardTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			int? year = ParamHelper.GetInt(pp, "year");
			var model = await _awardTasks.GetAwardsForModule(year);
			return View(model);
		}
	}
}