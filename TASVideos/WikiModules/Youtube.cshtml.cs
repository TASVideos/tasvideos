using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Youtube)]
public class Youtube : WikiViewComponent
{
	public string Code { get; set; } = "";
	public int Width { get; set; } = 425;
	public int Height { get; set; } = 370;
	public string Align { get; set; } = "";
	public int Start { get; set; }
	public int? Loop { get; set; }
	public bool HideLink { get; set; }
	public bool FlashBlock { get; set; }

	public IViewComponentResult Invoke(string v, int? loop, bool hideLink, bool flashblock, int? w, int? h, string? align, int? start)
	{
		Code = v;
		Loop = loop;
		HideLink = hideLink;
		FlashBlock = flashblock;

		if (w.HasValue)
		{
			Width = w.Value;
		}

		if (h.HasValue)
		{
			Height = h.Value;
		}

		if (!string.IsNullOrWhiteSpace(align)
			&& (align.Equals("left", StringComparison.InvariantCultureIgnoreCase)
				|| align.Equals("right", StringComparison.CurrentCultureIgnoreCase)))
		{
			Align = align.ToLower();
		}

		if (start.HasValue)
		{
			Start = start.Value;
		}

		return View();
	}
}
