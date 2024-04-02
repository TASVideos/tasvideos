namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.BanIpAddresses)]
public class IpBanModel(IIpBanService banService) : BasePageModel
{
	public ICollection<IpBanEntry> BannedIps { get; set; } = [];

	[FromQuery]
	public string? BanIp { get; set; }

	[BindProperty]
	[StringLength(40)]
	public string IpAddressToBan { get; set; } = "";

	public async Task OnGet()
	{
		IpAddressToBan = BanIp ?? "";
		await PopulateList();
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

	private async Task PopulateList() => BannedIps = await banService.GetAll();

	private RedirectToPageResult RedirectToIpBan() => RedirectToPage("/Users/IpBan");
}
