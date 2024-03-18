using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class ReferrersModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	public IEnumerable<WikiPageReferral> Referrals { get; set; } = [];

	public async Task OnGet()
	{
		Path = Path?.Trim('/') ?? "";
		Referrals = await db.WikiReferrals
			.ThatReferTo(Path)
			.ToListAsync();
		ViewData["PageName"] = Path;
	}
}
