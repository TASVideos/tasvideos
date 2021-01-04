using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class PlatformAuthorLists : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public PlatformAuthorLists(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(string pp)
		{
			var showTiers = ParamHelper.GetBool(pp, "showtiers");
			var before = ParamHelper.GetYear(pp, "before");
			var after = ParamHelper.GetYear(pp, "after");
			var platforms = ParamHelper.GetInts(pp, "platforms").ToList();

			if (!before.HasValue || !after.HasValue || !platforms.Any())
			{
				return new ContentViewComponentResult("Invalid paramters.");
			}

			var model = new PlatformAuthorListModel
			{
				ShowTiers = showTiers ?? false,
				Publications = await _db.Publications
					.ForYearRange(before.Value, after.Value)
					.Where(p => platforms.Contains(p.SystemId))
					.Select(p => new PlatformAuthorListModel.PublicationEntry
					{
						Id = p.Id,
						Title = p.Title,
						Authors = p.Authors.Select(pa => pa.Author!.UserName),
						TierIconPath = p.Tier!.IconPath
					})
					.ToListAsync()
			};

			return View(model);
		}
	}
}
