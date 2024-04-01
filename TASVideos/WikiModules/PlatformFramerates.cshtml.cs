using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PlatformFramerates)]
public class PlatformFramerates(ApplicationDbContext db) : WikiViewComponent
{
	public List<Framerate> Framerates { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Framerates = await db.GameSystemFrameRates
			.Where(sf => !sf.Obsolete)
			.Select(sf => new Framerate
			{
				SystemCode = sf.System!.Code,
				FrameRate = sf.FrameRate,
				RegionCode = sf.RegionCode,
				Preliminary = sf.Preliminary
			})
			.OrderBy(sf => sf.SystemCode)
			.ThenBy(sf => sf.RegionCode)
			.ToListAsync();
		return View();
	}

	public class Framerate
	{
		public string SystemCode { get; init; } = "";
		public string RegionCode { get; init; } = "";
		public double FrameRate { get; init; }
		public bool Preliminary { get; init; }
	}
}
