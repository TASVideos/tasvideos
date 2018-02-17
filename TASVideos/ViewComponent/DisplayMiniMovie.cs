using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class DisplayMiniMovie : ViewComponent
	{
		private readonly PublicationTasks _publicationTasks;

		public DisplayMiniMovie(PublicationTasks publicationTasks)
		{
			_publicationTasks = publicationTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			int id = new Random().Next(0, 2000); // TODO
			var orphans = await _publicationTasks.GetPublicationMiniMovie(id); // TODO
			return View(orphans);
		}
	}
}
