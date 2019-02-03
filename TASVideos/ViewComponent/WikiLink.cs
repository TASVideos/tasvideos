using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Extensions;

namespace TASVideos.ViewComponents
{
	public class WikiLink : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public WikiLink(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			pp = pp.Trim('/');
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
					model.Href = "Users/Profile/" + model.DisplayText;
				}
				else
				{
					var id = SubmissionHelper.IsSubmissionLink(pp);
					if (id.HasValue)
					{
						var title = await GetSubmissionTitle(id.Value);
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
							var title = $"[{mid.Value}]" + (await GetPublicationTitle(mid.Value));
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

		private async Task<string> GetPublicationTitle(int id)
		{
			return (await _db.Publications
				.Select(s => new { s.Id, s.Title })
				.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}

		private async Task<string> GetSubmissionTitle(int id)
		{
			return (await _db.Submissions
				.Select(s => new { s.Id, s.Title })
				.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}
	}
}