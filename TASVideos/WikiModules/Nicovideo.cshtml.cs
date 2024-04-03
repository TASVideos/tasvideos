using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Nicovideo)]
public class Nicovideo : WikiViewComponent
{
	public string Code { get; set; } = "";
	public int Width { get; set; } = 728;
	public int Height { get; set; } = 410;
	public string Align { get; set; } = "";
	public bool HideLink { get; set; }

	public IViewComponentResult Invoke(string v, bool hideLink, int? w, int? h, string? align)
	{
		Code = v;
		HideLink = hideLink;

		if (w.HasValue)
		{
			Width = w.Value;
		}

		if (h.HasValue)
		{
			Height = h.Value;
		}

		if (!string.IsNullOrWhiteSpace(align)
			&& (align.Equals("left", StringComparison.InvariantCultureIgnoreCase) || align.Equals("right", StringComparison.InvariantCultureIgnoreCase)))
		{
			Align = align.ToLower();
		}

		return View();
	}
}
