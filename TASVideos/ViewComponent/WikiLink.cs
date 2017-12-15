using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class WikiLink : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{

			string[] split = pp?.Split('|') ?? new[] { "", "" };
			var model = new WikiLinkModel
			{
				Href = split[0].Trim('/'),
				DisplayText = split.Length > 1 ? split[1] : split[0]
			};

			return View(model);
		}
	}
}