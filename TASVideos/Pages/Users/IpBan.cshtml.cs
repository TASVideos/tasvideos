using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.BanIpAddresses)]
public class IpBanModel : PageModel
{
	private readonly IIpBanService _banService;

	public IpBanModel(IIpBanService banService)
	{
		_banService = banService;
	}

	public IList<IpBanEntry> BannedIps { get; set; } = new List<IpBanEntry>();

	[FromQuery]
	public string? BanIp { get; set; }

	[BindProperty]
	[Required]
	[StringLength(40)]
	public string? IpAddress { get; set; }

	public async Task<IActionResult> OnGet()
	{
		IpAddress = BanIp;
		await PopulateList();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!string.IsNullOrWhiteSpace(IpAddress))
		{
			var result = await _banService.Add(IpAddress);
			if (!result)
			{
				ModelState.AddModelError(nameof(IpAddress), "Unable to add ip address or ip address range");
				await PopulateList();
				return Page();
			}
		}

		return RedirectToIpBan();
	}

	public async Task<IActionResult> OnPostDelete(string mask)
	{
		await _banService.Remove(mask);
		return RedirectToIpBan();
	}

	private async Task PopulateList()
	{
		BannedIps = (await _banService.GetAll())
			.OrderByDescending(b => b.DateCreated)
			.ToList();
	}

	private IActionResult RedirectToIpBan() => RedirectToPage("/Users/IpBan");
}
