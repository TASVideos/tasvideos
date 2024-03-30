using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Nicovideo)]
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

	public class NicovideoModel
	{
		public string Code { get; set; } = "";
		public int Width { get; set; } = 728;
		public int Height { get; set; } = 410;
		public string Align { get; set; } = "";
		public bool HideLink { get; set; }
	}
}
