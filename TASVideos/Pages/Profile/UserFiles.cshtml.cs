using TASVideos.Core;

namespace TASVideos.Pages.Profile;

[Authorize]
public class UserFilesModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public string UserName { get; set; } = "";

	public PageOf<UserFiles.InfoModel.UserFileModel> Files { get; set; } = PageOf<UserFiles.InfoModel.UserFileModel>.Empty();

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
