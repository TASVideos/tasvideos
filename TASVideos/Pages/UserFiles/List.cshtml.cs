namespace TASVideos.Pages.UserFiles;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public UserFileListRequest Search { get; set; } = new();

	public PageOf<UserFileEntry, UserFileListRequest> UserFiles { get; set; } = new([], new());
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
				Game = uf.Game != null ? uf.Game.DisplayName : "",
				Frames = uf.Frames,
				Rerecords = uf.Rerecords,
				Comments = uf.Comments.Count,
				Uploaded = uf.UploadTimestamp
			})
			.SortedPageOf(Search);
	}

	[PagingDefaults(PageSize = 50, Sort = $"-{nameof(UserFileEntry.Uploaded)}")]
	public class UserFileListRequest : PagingModel;

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
		public string Game { get; init; } = "";

		[Sortable]
		public int Frames { get; init; }

		[Sortable]
		public int Rerecords { get; init; }

		[Sortable]
		public int Comments { get; init; }

		[Sortable]
		public DateTime Uploaded { get; init; }
	}
}
