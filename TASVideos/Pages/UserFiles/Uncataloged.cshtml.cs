namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class Uncataloged(ApplicationDbContext db) : BasePageModel
{
	public List<UncatalogedViewModel> Files { get; set; } = [];

	public async Task OnGet()
	{
		Files = await db.UserFiles
			.ThatArePublic()
			.Where(uf => uf.GameId == null)
			.ToUnCatalogedModel()
			.ToListAsync();
	}

	public record UncatalogedViewModel(
		long Id,
		string FileName,
		string? SystemCode,
		DateTime UploadTimestamp,
		string Author);
}
