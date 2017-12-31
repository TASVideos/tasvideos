using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;

namespace TASVideos.ViewComponents
{
	public class Youtube : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			var model = new YoutubeModel
			{
				Code = WikiHelper.GetValueFor(pp, "v"),
				Loop = WikiHelper.GetInt(WikiHelper.GetValueFor(pp, "loop")),
				HideLink = WikiHelper.HasParam(pp, "hidelink"),
				FlashBlock = WikiHelper.HasParam(pp, "flashblock")
			};

			int? paramWidth = WikiHelper.GetInt(WikiHelper.GetValueFor(pp, "width"));
			if (paramWidth.HasValue)
			{
				model.Width = paramWidth.Value;
			}

			int? paramHeight = WikiHelper.GetInt(WikiHelper.GetValueFor(pp, "height"));
			if (paramHeight.HasValue)
			{
				model.Height = paramHeight.Value;
			}

			string paramAlign = WikiHelper.GetValueFor(pp, "align");
			if (!string.IsNullOrWhiteSpace(paramAlign)
				&& (paramAlign.ToLower() == "left" || paramAlign.ToLower() == "right"))
			{
				model.Align = paramAlign.ToLower();
			}

			int? startParam = WikiHelper.GetInt(WikiHelper.GetValueFor(pp, "start"));
			if (startParam.HasValue)
			{
				model.Start = startParam.Value;
			}

			return View(model);
		}
	}
}
