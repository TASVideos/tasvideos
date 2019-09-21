using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class UnmirroredMovies : ViewComponent
	{
		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var model = new object();

			return View(model);
		}
	}
}
