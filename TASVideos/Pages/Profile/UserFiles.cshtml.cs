﻿using TASVideos.Core;

namespace TASVideos.Pages.Profile;

[Authorize]
public class UserFilesModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<UserFiles.InfoModel.UserFileModel> Files { get; set; } = PageOf<UserFiles.InfoModel.UserFileModel>.Empty();

	public async Task OnGet()
	{
		Files = await db.UserFiles
			.ForAuthor(User.Name())
			.OrderByDescending(uf => uf.UploadTimestamp)
			.ToUserFileModel()
			.PageOf(Search);
	}
}
