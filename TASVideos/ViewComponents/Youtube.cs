using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Youtube)]
public class Youtube : ViewComponent
{
	public IViewComponentResult Invoke(string v, int? loop, bool hidelink, bool flashblock, int? w, int? h, string? align, int? start)
	{
		var model = new YoutubeModel
		{
			Code = v,
			Loop = loop,
			HideLink = hidelink,
			FlashBlock = flashblock,
		};

		if (w.HasValue)
		{
			model.Width = w.Value;
		}

		if (h.HasValue)
		{
			model.Height = h.Value;
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
