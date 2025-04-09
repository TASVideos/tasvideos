namespace TASVideos.Pages.Account;

public class BannedModel(ApplicationDbContext db) : BasePageModel
{
	[TempData]
	public int? BannedUserId { get; set; }

	public string UserName { get; set; } = "";
	public DateTime? BannedUntil { get; set; }
	public bool BanIsIndefinite => BannedUntil >= DateTime.UtcNow.AddYears(SiteGlobalConstants.YearsOfBanDisplayedAsIndefinite);

	public async Task<IActionResult> OnGet()
	{
		if (BannedUserId is null)
		{
			return Home();
		}

		var user = await db.Users
			.Where(u => u.Id == BannedUserId)
			.Select(u => new
			{
				u.UserName,
				u.BannedUntil
			})
			.FirstOrDefaultAsync();

		if (user is null)
		{
			return Home();
		}

		UserName = user.UserName;
		BannedUntil = user.BannedUntil;

		return Page();
	}
}
