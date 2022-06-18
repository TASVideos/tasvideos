using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Systems;

[RequirePermission(PermissionTo.GameSystemMaintenance)]
public class CreateFramerateModel : BasePageModel
{
	private readonly IGameSystemService _systemService;
	private readonly ApplicationDbContext _db;

	public CreateFramerateModel(IGameSystemService systemService, ApplicationDbContext db)
	{
		_systemService = systemService;
		_db = db;
	}

	[FromRoute]
	public int SystemId { get; set; }

	[BindProperty]
	public string SystemCode { get; set; } = "";

	[BindProperty]
	public double FrameRate { get; set; }

	[BindProperty]
	[Required]
	[StringLength(8)]
	public string RegionCode { get; set; } = "";

	[BindProperty]
	public bool Preliminary { get; set; }

	[BindProperty]
	public bool Obsolete { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var system = await _systemService.GetById(SystemId);

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

		var system = await _systemService.GetById(SystemId);

		if (system is null)
		{
			return NotFound();
		}

		_db.Add(new GameSystemFrameRate
		{
			GameSystemId = SystemId,
			FrameRate = FrameRate,
			RegionCode = RegionCode,
			Preliminary = Preliminary,
			Obsolete = false
		});

		await ConcurrentSave(
			_db,
			$"Framerate successfully created for {SystemCode}",
			$"Unable to create Framerate for {SystemCode}");
		await _systemService.FlushCache();

		return BasePageRedirect("Index");
	}
}
