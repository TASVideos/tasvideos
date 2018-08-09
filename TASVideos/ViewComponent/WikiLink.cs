using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class WikiLink : ViewComponent
	{
		private readonly SubmissionTasks _submissionTasks;
		private readonly PublicationTasks _publicationTasks;

		public WikiLink(SubmissionTasks submissionTasks, PublicationTasks publicationTasks)
		{
			_submissionTasks = submissionTasks;
			_publicationTasks = publicationTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var split = pp.Split('|');

			var model = new WikiLinkModel
			{
				Href = WikiHelper.NormalizeWikiPageName(split[0]),
				DisplayText = split.Length > 1 ? split[1] : split[0]
			};

			if (split.Length == 1)
			{
				if (pp.StartsWith("user:"))
				{
					model.DisplayText = model.DisplayText.Substring(5);
					model.Href = "User/Profile/" + model.DisplayText;
				}
				else
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
					else
					{
						var mid = SubmissionHelper.IsPublicationLink(pp);
						if (mid.HasValue)
						{
							var title = $"[{mid.Value}]" + (await _publicationTasks.GetTitle(mid.Value));
							if (!string.IsNullOrWhiteSpace(title))
							{
								model.DisplayText = title;
							}
						}
					}
				}
			}

			return View(model);
		}
	}
}