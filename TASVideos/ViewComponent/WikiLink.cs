using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class WikiLink : ViewComponent
	{
		private readonly SubmissionTasks _submissionTasks;

		public WikiLink(SubmissionTasks submissionTasks)
		{
			_submissionTasks = submissionTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			string[] split = pp.Split('|');

			var model = new WikiLinkModel
			{
				Href = split[0]
					.Trim('/')
					.Replace(" ", "")
					.Replace(".html", ""),
				DisplayText = split.Length > 1 ? split[1] : split[0]
			};

			if (split.Length == 1)
			{
				var id = SubmissionHelper.IsSubmissionLink(pp);
				if (id.HasValue)
				{
					var title = await _submissionTasks.GetTitle(id.Value);
					if (!string.IsNullOrWhiteSpace(title))
					{
						model.DisplayText = title;
					}
				}
			}


			return View(model);
		}
	}
}