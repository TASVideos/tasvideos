using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class Youtube : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			var model = new YoutubeModel
			{
				Code = ParamHelper.GetValueFor(pp, "v"),
				Loop = ParamHelper.GetInt(pp, "loop"),
				HideLink = ParamHelper.HasParam(pp, "hidelink"),
				FlashBlock = ParamHelper.HasParam(pp, "flashblock")
			};

			int? paramWidth = ParamHelper.GetInt(pp, "width");
			if (paramWidth.HasValue)
			{
				model.Width = paramWidth.Value;
			}

			int? paramHeight = ParamHelper.GetInt(pp, "height");
			if (paramHeight.HasValue)
			{
				model.Height = paramHeight.Value;
			}

			string paramAlign = ParamHelper.GetValueFor(pp, "align");
			if (!string.IsNullOrWhiteSpace(paramAlign)
				&& (paramAlign.ToLower() == "left" || paramAlign.ToLower() == "right"))
			{
				model.Align = paramAlign.ToLower();
			}

			int? startParam = ParamHelper.GetInt(pp, "start");
			if (startParam.HasValue)
			{
				model.Start = startParam.Value;
			}

			return View(model);
		}
	}
}
