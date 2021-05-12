using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.RazorPages.ViewComponents
{
	[WikiModule(WikiModules.Youtube)]
	public class Youtube : ViewComponent
	{
		public IViewComponentResult Invoke(string v, int? loop, bool hidelink, bool flashblock, int? width, int? height, string? align, int? start)
		{
			var model = new YoutubeModel
			{
				Code = v,
				Loop = loop,
				HideLink = hidelink,
				FlashBlock = flashblock,
			};

			if (width.HasValue)
			{
				model.Width = width.Value;
			}

			if (height.HasValue)
			{
				model.Height = height.Value;
			}

			if (!string.IsNullOrWhiteSpace(align)
				&& (align.ToLower() == "left" || align.ToLower() == "right"))
			{
				model.Align = align.ToLower();
			}

			if (start.HasValue)
			{
				model.Start = start.Value;
			}

			return View(model);
		}
	}
}
