using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Systems;

[RequirePermission(PermissionTo.GameSystemMaintenance)]
public class CreateFramerateModel(IGameSystemService systemService, ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int SystemId { get; set; }

	[BindProperty]
	public string SystemCode { get; set; } = "";

	[BindProperty]
	public double FrameRate { get; set; }

	[BindProperty]
	[StringLength(8)]
	public string RegionCode { get; set; } = "";

	[BindProperty]
	public bool Preliminary { get; set; }

	[BindProperty]
	public bool Obsolete { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var system = await systemService.GetById(SystemId);
		if (system is null)
		{
			return NotFound();
		}

		SystemCode = system.Code;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var system = await systemService.GetById(SystemId);
		if (system is null)
		{
			return NotFound();
		}

		db.Add(new GameSystemFrameRate
		{
			GameSystemId = SystemId,
			FrameRate = FrameRate,
			RegionCode = RegionCode,
			Preliminary = Preliminary,
			Obsolete = false
		});

		SetMessage(
			await db.TrySaveChanges(),
			$"Framerate successfully created for {SystemCode}",
			$"Unable to create Framerate for {SystemCode}");
		await systemService.FlushCache();

		return BasePageRedirect("Edit", new { Id = SystemId });
	}
}
