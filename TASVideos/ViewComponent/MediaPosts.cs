using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
    public class MediaPosts : ViewComponent
    {
		private readonly MediaTasks _mediaTasks;

		public MediaPosts(MediaTasks mediaTasks)
		{
			_mediaTasks = mediaTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			int days = ParamHelper.GetInt(pp, "days") ?? 7;
			var startDate = DateTime.Now.AddDays(-days);
			var limit = ParamHelper.GetInt(pp, "limit") ?? 50;
			var model = await _mediaTasks.GetPosts(startDate, limit);

			return View(model);
		}
	}
}
