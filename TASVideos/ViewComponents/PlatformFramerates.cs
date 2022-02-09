using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PlatformFramerates)]
public class PlatformFramerates : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public PlatformFramerates(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var model = await _db.GameSystemFrameRates
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
