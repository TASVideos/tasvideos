namespace TASVideos.Pages.UserFiles;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public UserFileListRequest Search { get; set; } = new();

	public PageOf<UserFileEntry> UserFiles { get; set; } = PageOf<UserFileEntry>.Empty();
	public async Task OnGet()
	{
		UserFiles = await db.UserFiles
			.ThatArePublic()
			.ByRecentlyUploaded()
			.Select(uf => new UserFileEntry(
				uf.Id,
				uf.Title,
				uf.FileName,
				uf.Author!.UserName,
				uf.GameId,
				uf.Game != null ? uf.Game.DisplayName : "",
				uf.Frames,
				uf.Rerecords,
				uf.Comments.Count,
				uf.UploadTimestamp))
			.SortedPageOf(Search);
	}

	public class UserFileListRequest : PagingModel
	{
		public UserFileListRequest()
		{
			PageSize = 50;
			Sort = $"-{nameof(UserFileEntry.Uploaded)}";
		}
	}

	public record UserFileEntry(
		[property: TableIgnore]long Id,
		string Title,
		[property: TableIgnore]string FileName,
		[property: Sortable] string Author,
		[property: TableIgnore] int? GameId,
		[property: Sortable] string Game,
		[property: Sortable] int Frames,
		[property: Sortable] int Rerecords,
		[property: Sortable] int Comments,
		[property: Sortable] DateTime Uploaded);
}
