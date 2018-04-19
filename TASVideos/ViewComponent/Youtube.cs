using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class Youtube : ModuleComponentBase
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			var model = new YoutubeModel
			{
				Code = GetValueFor(pp, "v"),
				Loop = GetInt(pp, "loop"),
				HideLink = HasParam(pp, "hidelink"),
				FlashBlock = HasParam(pp, "flashblock")
			};

			int? paramWidth = GetInt(pp, "width");
			if (paramWidth.HasValue)
			{
				model.Width = paramWidth.Value;
			}

			int? paramHeight = GetInt(pp, "height");
			if (paramHeight.HasValue)
			{
				model.Height = paramHeight.Value;
			}

			string paramAlign = GetValueFor(pp, "align");
			if (!string.IsNullOrWhiteSpace(paramAlign)
				&& (paramAlign.ToLower() == "left" || paramAlign.ToLower() == "right"))
			{
				model.Align = paramAlign.ToLower();
			}

			int? startParam = GetInt(pp, "start");
			if (startParam.HasValue)
			{
				model.Start = startParam.Value;
			}

			return View(model);
		}
	}
}
