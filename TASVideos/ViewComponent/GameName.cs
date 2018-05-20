using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class GameName : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			// TODO: this needs to look up from the Game table
			// Also, for pages like GameResources/NES it could look up the system name
			return View();
		}
	}
}
