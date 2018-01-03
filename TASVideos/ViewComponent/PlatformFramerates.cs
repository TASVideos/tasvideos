using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class PlatformFramerates : ViewComponent
	{
		private readonly PlatformTasks _platFormTasks;

		public PlatformFramerates(PlatformTasks platFormTasks)
		{
			_platFormTasks = platFormTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var model = await _platFormTasks.GetAllPlatformFrameRates();
			return View(model);
		}
	}
}