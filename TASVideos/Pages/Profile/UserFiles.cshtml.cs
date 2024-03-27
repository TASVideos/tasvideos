using TASVideos.Core;
using TASVideos.Models;

namespace TASVideos.Pages.Profile;

[Authorize]
public class UserFilesModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public string UserName { get; set; } = "";

	public PageOf<UserFileModel> Files { get; set; } = PageOf<UserFileModel>.Empty();

	public async Task OnGet()
	{
		UserName = User.Name();
		Files = await db.UserFiles
			.ForAuthor(UserName)
			.OrderByDescending(uf => uf.UploadTimestamp)
			.ToUserFileModel()
			.PageOf(Search);
	}
}
