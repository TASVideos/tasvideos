using AutoMapper;
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
	private readonly IMapper _mapper;

	public EditModel(ApplicationDbContext db, IMapper mapper)
		: base(db)
	{
		_db = db;
		_mapper = mapper;
	}

	[FromRoute]
	public int Id { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var address = await _mapper.ProjectTo<AddressEditModel>(_db.GameRamAddresses
			.Include(r => r.System)
			.Include(r => r.Game)
			.Where(r => r.Id == Id))
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
