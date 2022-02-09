using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.RamAddresses.Models;

namespace TASVideos.Pages.RamAddresses;

public class AddressBasePageModel : BasePageModel
{
	protected readonly ApplicationDbContext Db;

	public AddressBasePageModel(ApplicationDbContext db)
	{
		Db = db;
	}

	private static readonly IEnumerable<RamAddressType> AddressTypes = Enum
		.GetValues<RamAddressType>();

	private static readonly IEnumerable<RamAddressSigned> AddressSigned = Enum
		.GetValues<RamAddressSigned>();

	private static readonly IEnumerable<RamAddressEndian> AddressEndian = Enum
		.GetValues<RamAddressEndian>();

	public List<SelectListItem> Domains { get; set; } = new();

	public List<SelectListItem> Types { get; } = AddressTypes
		.Select(t => new SelectListItem
		{
			Value = ((int)t).ToString(),
			Text = t.ToString()
		})
		.ToList();

	public List<SelectListItem> SignedOptions { get; } = AddressSigned
		.Select(t => new SelectListItem
		{
			Value = ((int)t).ToString(),
			Text = t.ToString()
		})
		.ToList();

	public List<SelectListItem> EndianOptions { get; } = AddressEndian
		.Select(t => new SelectListItem
		{
			Value = ((int)t).ToString(),
			Text = t.ToString()
		})
		.ToList();

	[BindProperty]
	public AddressEditModel Address { get; set; } = new();

	protected IActionResult RedirectToList(int gameId)
	{
		return BasePageRedirect("Index", new { Id = gameId });
	}

	protected async Task PopulateDropdowns(int systemId)
	{
		Domains = await Db.GameRamAddressDomains
			.Where(d => d.GameSystemId == systemId)
			.Select(d => new SelectListItem
			{
				Text = d.Name,
				Value = d.GameSystemId.ToString()
			})
			.ToListAsync();
	}

	protected async Task ValidateDomain()
	{
		var domain = await Db.GameRamAddressDomains
			.SingleOrDefaultAsync(d => d.Id == Address.GameRamAddressDomainId);

		if (domain == null)
		{
			ModelState.AddModelError($"{nameof(Address)}.{nameof(Address.GameRamAddressDomainId)}", "Domain does not exist.");
		}
	}
}
