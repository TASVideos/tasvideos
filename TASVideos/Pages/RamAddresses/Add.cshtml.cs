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
			.SingleOrDefaultAsync(g => g.Id == GameId);

		if (game == null)
		{
			return NotFound();
		}

		SystemId = game.SystemId;
		await PopulateDropdowns(SystemId);

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			var game = await _db.Games
				.SingleOrDefaultAsync(g => g.Id == GameId);

			if (game == null)
			{
				return NotFound();
			}

			SystemId = game.SystemId;
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
