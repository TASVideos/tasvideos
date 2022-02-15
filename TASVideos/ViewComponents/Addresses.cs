using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Addresses)]
public class Addresses : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public Addresses(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(int addrset)
	{
		var model = await _db.GameRamAddresses
			.Where(a => a.LegacySetId == addrset)
			.Select(a => new AddressEntry(
				a.GameRamAddressDomain!.Name,
				a.Address,
				a.Type.ToString(),
				a.Signed.ToString(),
				a.Endian.ToString(),
				a.Description,
				a.Game != null ? a.Game.DisplayName : null,
				a.GameId,
				a.Game != null ? a.Game.System!.DisplayName : null))
			.ToListAsync();

		if (!model.Any())
		{
			return new ContentViewComponentResult("Invalid addrset.");
		}

		return View(model);
	}
}
