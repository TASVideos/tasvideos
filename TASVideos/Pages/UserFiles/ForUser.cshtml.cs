using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class ForUserModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	[FromRoute]
	public string UserName { get; set; } = "";

	public IEnumerable<UserFileModel> Files { get; set; } = [];

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
