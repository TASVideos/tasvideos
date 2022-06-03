using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.RamAddresses;

[RequirePermission(PermissionTo.EditRamAddresses)]
public class AddModel : AddressBasePageModel
{
	private readonly ApplicationDbContext _db;

	public AddModel(ApplicationDbContext db)
		: base(db)
	{
		_db = db;
	}

	[FromRoute]
	public int GameId { get; set; }

	[BindProperty]
	public int SystemId { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var game = await _db.Games
			.Include(g => g.GameVersions)
			.SingleOrDefaultAsync(g => g.Id == GameId);

		if (game is null)
		{
			return NotFound();
		}

		// HACK: Games no longer have a System, so this takes the System from existing Addresses of the Game
		// If there are none it takes the first Game Version and gets the System from there.
		// The correct solution would be to rework the RAM Addresses system to be assigned to a Game Version instead of a Game.
		// This would also require rewiring all existing RAM Addresses in the database.
		var systemId = await _db.GameRamAddresses
			.Where(a => a.GameId == GameId)
			.Select(a => a.SystemId)
			.FirstOrDefaultAsync();
		if (systemId == 0)
		{
			systemId = game.GameVersions.FirstOrDefault()?.SystemId ?? 1;
		}

		SystemId = systemId;
		await PopulateDropdowns(SystemId);

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			var game = await _db.Games
				.SingleOrDefaultAsync(g => g.Id == GameId);

			if (game is null)
			{
				return NotFound();
			}

			// HACK: Same as above
			var systemId = await _db.GameRamAddresses
			.Where(a => a.GameId == GameId)
			.Select(a => a.SystemId)
			.FirstOrDefaultAsync();
			if (systemId == 0)
			{
				systemId = game.GameVersions.FirstOrDefault()?.SystemId ?? 1;
			}

			SystemId = systemId;
			await PopulateDropdowns(SystemId);
			return Page();
		}

		await ValidateDomain();

		_db.GameRamAddresses.Add(new GameRamAddress
		{
			GameId = GameId,
			SystemId = SystemId,
			GameRamAddressDomainId = Address.GameRamAddressDomainId,
			Address = Address.Address,
			Type = Address.Type ?? RamAddressType.Byte,
			Signed = Address.Signed ?? RamAddressSigned.Signed,
			Endian = Address.Endian ?? RamAddressEndian.Big,
			Description = Address.Description
		});

		await ConcurrentSave(_db, "Address successfully added", "Unable to modify address");
		return RedirectToList(GameId);
	}
}
