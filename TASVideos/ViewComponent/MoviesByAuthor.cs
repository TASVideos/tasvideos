using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.ViewComponents.Models;

namespace TASVideos.ViewComponents
{
	public class MoviesByAuthor : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public MoviesByAuthor(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(string pp)
		{
			var before = ParamHelper.GetYear(pp, "before");
			var after = ParamHelper.GetYear(pp, "after");

			if (!before.HasValue || !after.HasValue)
			{
				return View(new MoviesByAuthorModel());
			}

			var newbieFlag = ParamHelper.GetValueFor(pp, "newbies").ToLower();
			var newbiesOnly = newbieFlag == "only";

			var model = new MoviesByAuthorModel
			{
				MarkNewbies = newbieFlag == "show",
				ShowTiers = ParamHelper.HasParam(pp, "showtiers"),
				Publications = await _db.Publications
					.Where(p => p.CreateTimeStamp.Year < before.Value)
					.Where(p => p.CreateTimeStamp.Year >= after.Value)
					.Select(p => new MoviesByAuthorModel.PublicationEntry
					{
						Id = p.Id,
						Title = p.Title,
						Authors = p.Authors.Select(pa => pa.Author!.UserName),
						TierIconPath = p.Tier!.IconPath
					})
					.ToListAsync()
			};

			if (newbiesOnly || model.MarkNewbies)
			{
				model.NewbieAuthors = await _db.Users
					.ThatArePublishedAuthors()
					.Where(u => u.Publications
						.OrderBy(p => p.Publication!.CreateTimeStamp)
						.First().Publication!.CreateTimeStamp.Year == after)
					.Select(u => u.UserName)
					.ToListAsync();
			}

			if (newbiesOnly)
			{
				model.Publications = model.Publications
					.Where(p => p.Authors.Any(a => model.NewbieAuthors.Contains(a)))
					.ToList();
			}

			return View(model);
		}
	}
}
