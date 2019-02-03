using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class Awards : ViewComponent
	{
		private readonly IAwardsCache _awards;

		public Awards(IAwardsCache awards)
		{
			_awards = awards;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var cache = ParamHelper.GetValueFor(pp, "clear");
			if (!string.IsNullOrWhiteSpace(cache))
			{
				return new ContentViewComponentResult("Error: clear parameter no longer supported");
			}

			int? year = ParamHelper.GetInt(pp, "year");

			if (!year.HasValue)
			{
				return new ContentViewComponentResult("Error: parameterless award module no longer supported");
			}

			var allAwards = await _awards.Awards();
			var model = allAwards
				.Where(a => a.Year + 2000 == year)
				.ToList();

			return View(model);
		}
	}
}