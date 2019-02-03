using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class PlatformFramerates : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public PlatformFramerates(
			ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var model = await _db.GameSystemFrameRates
				.Select(sf => new PlatformFramerateModel
				{
					SystemCode = sf.System.Code,
					FrameRate = sf.FrameRate,
					RegionCode = sf.RegionCode,
					Preliminary = sf.Preliminary
				})
				.OrderBy(sf => sf.SystemCode)
				.ThenBy(sf => sf.RegionCode)
				.ToListAsync();
			return View(model);
		}
	}
}