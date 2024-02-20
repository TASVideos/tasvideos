using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.Profile;

[Authorize]
public class UserFilesModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public UserFilesModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public string UserName { get; set; } = "";

	public PageOf<UserFileModel> Files { get; set; } = PageOf<UserFileModel>.Empty();

	public async Task OnGet()
	{
		UserName = User.Name();
		Files = await _db.UserFiles
			.ForAuthor(UserName)
			.OrderByDescending(uf => uf.UploadTimestamp)
			.ToUserFileModel()
			.PageOf(Search);
	}
}
