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
			.OrderBy(sf => sf.System!.Code)
			.ThenBy(sf => sf.RegionCode)
			.Select(sf => new Framerate(
				sf.System!.Code, sf.RegionCode, sf.FrameRate, sf.Preliminary))
			.ToListAsync();
		return View();
	}

	public record Framerate(string SystemCode, string RegionCode, double FrameRate, bool Preliminary);
}
