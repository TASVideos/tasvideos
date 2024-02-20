using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

public class ListModel : PageModel
{
	private readonly ApplicationDbContext _db;
	public ListModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromQuery]
	public UserFileListRequest Search { get; set; } = new();

	public PageOf<UserFileListModel> UserFiles { get; set; } = PageOf<UserFileListModel>.Empty();
	public async Task OnGet()
	{
		UserFiles = await _db.UserFiles
			.ThatArePublic()
			.ByRecentlyUploaded()
			.Select(uf => new UserFileListModel
			{
				Id = uf.Id,
				Title = uf.Title,
				FileName = uf.FileName,
				Author = uf.Author!.UserName,
				GameId = uf.GameId,
				GameName = uf.Game != null ? uf.Game.DisplayName : "",
				Frames = uf.Frames,
				Rerecords = uf.Rerecords,
				CommentCount = uf.Comments.Count,
				UploadTimestamp = uf.UploadTimestamp,
			})
			.SortedPageOf(Search);
	}
}
