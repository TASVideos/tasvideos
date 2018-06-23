using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Tasks;
using TASVideos.WikiEngine;

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
			string[] split = pp.Split('|');

			var model = new WikiLinkModel
			{
				Href = split[0]
					.Trim('/')
					.Replace(".html", ""),
				DisplayText = split.Length > 1 ? split[1] : split[0]
			};

			if (!model.Href.StartsWith("user:"))
			{
				model.Href = model.Href.Replace(" ", "");
			}

			if (split.Length == 1)
			{
				if (pp.StartsWith("user:"))
				{
					model.DisplayText = model.DisplayText.Replace("user:", "");
					model.Href = Uri.EscapeUriString(Util.RenderUserModuleLink(model.Href));
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