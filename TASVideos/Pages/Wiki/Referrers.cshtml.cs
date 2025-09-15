namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class ReferrersModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	public List<WikiPageReferral> Referrals { get; set; } = [];

	public async Task OnGet()
	{
		Path = Path?.Trim('/') ?? "";
		if (!string.IsNullOrWhiteSpace(Path))
		{
			Referrals = await db.WikiReferrals
				.AsNoTracking()
				.ThatReferTo(Path)
				.ToListAsync();
		}
	}
}
