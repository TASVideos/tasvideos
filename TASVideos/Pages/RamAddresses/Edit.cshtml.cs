using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.RamAddresses.Models;

namespace TASVideos.Pages.RamAddresses;

[RequirePermission(PermissionTo.EditRamAddresses)]
public class EditModel : AddressBasePageModel
{
	private readonly ApplicationDbContext _db;

	public EditModel(ApplicationDbContext db)
		: base(db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var address = await _db.GameRamAddresses
			.Where(r => r.Id == Id)
			.Select(r => new AddressEditModel
			{
				GameRamAddressDomainId = r.GameRamAddressDomainId,
				Address = r.Address,
				Type = r.Type,
				Signed = r.Signed,
				Endian = r.Endian,
				Description = r.Description,
				GameId = r.GameId ?? 0,
				GameName = r.Game!.DisplayName,
				SystemCode = r.System!.Code,
				SystemId = r.SystemId
			})
			.SingleOrDefaultAsync();

		if (address is null)
		{
			return NotFound();
		}

		Address = address;
		await PopulateDropdowns(Address.SystemId);

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await PopulateDropdowns(Address.SystemId);
			return Page();
		}

		var address = await _db.GameRamAddresses
			.SingleOrDefaultAsync(a => a.Id == Id);

		if (address is null)
		{
			return NotFound();
		}

		await ValidateDomain();

		address.GameRamAddressDomainId = Address.GameRamAddressDomainId;
		address.Address = Address.Address;
		address.Type = Address.Type ?? RamAddressType.Byte;
		address.Signed = Address.Signed ?? RamAddressSigned.Signed;
		address.Endian = Address.Endian ?? RamAddressEndian.Big;
		address.Description = Address.Description;

		await ConcurrentSave(_db, "Address successfully updated", "Unable to modify address");
		return RedirectToList(Address.GameId);
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var address = await _db.GameRamAddresses
			.SingleOrDefaultAsync(a => a.Id == Id);

		if (address is null)
		{
			return NotFound();
		}

		_db.GameRamAddresses.Remove(address);
		await ConcurrentSave(_db, $"Address {address.Address} successfully deleted.", "Unable to delete address.");
		return RedirectToList(address.GameId!.Value);
	}
}
