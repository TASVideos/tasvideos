using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core;

namespace TASVideos.Pages.UserFiles;

public class ListModel(ApplicationDbContext db) : PageModel
{
	[FromQuery]
	public UserFileListRequest Search { get; set; } = new();

	public PageOf<UserFileEntry> UserFiles { get; set; } = PageOf<UserFileEntry>.Empty();
	public async Task OnGet()
	{
		UserFiles = await db.UserFiles
			.ThatArePublic()
			.ByRecentlyUploaded()
			.Select(uf => new UserFileEntry
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

	public class UserFileListRequest : PagingModel
	{
		public UserFileListRequest()
		{
			PageSize = 50;
			Sort = $"-{nameof(UserFileEntry.UploadTimestamp)}";
		}
	}

	public class UserFileEntry
	{
		[TableIgnore]
		public long Id { get; init; }
		public string Title { get; init; } = "";

		[TableIgnore]
		public string FileName { get; init; } = "";

		[Sortable]
		public string Author { get; init; } = "";

		[TableIgnore]
		public int? GameId { get; init; }

		[Sortable]
		[Display(Name = "Game")]
		public string GameName { get; init; } = "";

		[Sortable]
		public int Frames { get; init; }

		[Sortable]
		public int Rerecords { get; init; }

		[Sortable]
		[Display(Name = "Comments")]
		public int CommentCount { get; init; }

		[Sortable]
		[Display(Name = "Uploaded")]
		public DateTime UploadTimestamp { get; init; }
	}
}
