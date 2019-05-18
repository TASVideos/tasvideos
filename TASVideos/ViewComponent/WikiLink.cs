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
			var split = pp.Split('|');
			var model = new WikiLinkModel
			{
				Href = split[0],
				DisplayText = split.Length == 1
					? split[0].Substring(1) // almost always want to chop off the leading '/'
					: split[1]
			};

			int? id;

			if (model.DisplayText.StartsWith("user:"))
			{
				model.DisplayText = model.DisplayText.Substring(5);
			}
			else if ((id = SubmissionHelper.IsSubmissionLink(split[0])).HasValue)
			{
				var title = await GetSubmissionTitle(id.Value);
				if (!string.IsNullOrWhiteSpace(title))
				{
					model.DisplayText = title;
				}
			}
			else if ((id = SubmissionHelper.IsPublicationLink(split[0])).HasValue)
			{
				var title = $"[{id.Value}]" + (await GetPublicationTitle(id.Value));
				if (!string.IsNullOrWhiteSpace(title))
				{
					model.DisplayText = title;
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