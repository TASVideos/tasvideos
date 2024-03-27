using TASVideos.Core;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class ForUserModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	[FromRoute]
	public string UserName { get; set; } = "";

	public PageOf<UserFileModel> Files { get; set; } = PageOf<UserFileModel>.Empty();

	public async Task OnGet()
	{
		Files = await db.UserFiles
			.ForAuthor(UserName)
			.HideIfNotAuthor(User.GetUserId())
			.OrderByDescending(uf => uf.UploadTimestamp)
			.ToUserFileModel()
			.PageOf(Search);
	}
}
