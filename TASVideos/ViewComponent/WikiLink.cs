using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class WikiLink : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			string[] split = pp.Split('|');
			var model = new WikiLinkModel
			{
				Href = split[0]
					.Trim('/')
					.Replace(" ", "")
					.Replace(".html", ""),
				DisplayText = split.Length > 1 ? split[1] : split[0]
			};

			return View(model);
		}
	}
}