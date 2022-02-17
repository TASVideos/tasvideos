using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Nicovideo)]
public class Nicovideo : ViewComponent
{
	public IViewComponentResult Invoke(string v, bool hideLink, int? w, int? h, string? align)
	{
		var model = new NicovideoModel
		{
			Code = v,
			HideLink = hideLink,
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

		return View(model);
	}
}
