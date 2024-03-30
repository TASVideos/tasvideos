using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Youtube)]
public class Youtube : ViewComponent
{
	public IViewComponentResult Invoke(string v, int? loop, bool hideLink, bool flashblock, int? w, int? h, string? align, int? start)
	{
		var model = new YoutubeModel
		{
			Code = v,
			Loop = loop,
			HideLink = hideLink,
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

	public class YoutubeModel
	{
		public string Code { get; set; } = "";
		public int Width { get; set; } = 425;
		public int Height { get; set; } = 370;
		public string Align { get; set; } = "";
		public int Start { get; set; }
		public int? Loop { get; set; }
		public bool HideLink { get; set; }
		public bool FlashBlock { get; set; }
	}
}
