using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.BanIpAddresses)]
public class IpBanModel(IIpBanService banService) : BasePageModel
{
	public IList<IpBanEntry> BannedIps { get; set; } = [];

	[FromQuery]
	public string? BanIp { get; set; }

	[BindProperty]
	[Required]
	[StringLength(40)]
	public string? IpAddressToBan { get; set; }

	public async Task<IActionResult> OnGet()
	{
		IpAddressToBan = BanIp;
		await PopulateList();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!string.IsNullOrWhiteSpace(IpAddressToBan))
		{
			var result = await banService.Add(IpAddressToBan);
			if (!result)
			{
				ModelState.AddModelError(nameof(IpAddressToBan), "Unable to add ip address or ip address range");
				await PopulateList();
				return Page();
			}
		}

		return RedirectToIpBan();
	}

	public async Task<IActionResult> OnPostDelete(string mask)
	{
		await banService.Remove(mask);
		return RedirectToIpBan();
	}

	private async Task PopulateList()
	{
		BannedIps = [.. (await banService.GetAll()).OrderByDescending(b => b.DateCreated)];
	}

	private RedirectToPageResult RedirectToIpBan() => RedirectToPage("/Users/IpBan");
}
