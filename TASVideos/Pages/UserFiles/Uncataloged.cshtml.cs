using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class Uncataloged(ApplicationDbContext db) : BasePageModel
{
	public List<UncatalogedViewModel> Files { get; set; } = [];

	[FromRoute]
	public int Id { get; set; }

	public async Task OnGet()
	{
		Files = await db.UserFiles
			.ThatArePublic()
			.Where(uf => uf.GameId == null)
			.ToUnCatalogedModel()
			.ToListAsync();
	}
}
