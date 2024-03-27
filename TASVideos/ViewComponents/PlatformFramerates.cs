using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PlatformFramerates)]
public class PlatformFramerates(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var model = await db.GameSystemFrameRates
			.Where(sf => !sf.Obsolete)
			.Select(sf => new PlatformFramerateModel
			{
				SystemCode = sf.System!.Code,
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
