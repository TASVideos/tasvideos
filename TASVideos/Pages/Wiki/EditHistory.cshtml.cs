namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class EditHistoryModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Paging { get; set; } = new();

	[FromRoute]
	public string UserName { get; set; } = "";

	public PageOf<HistoryEntry> History { get; set; } = new([], new());

	public async Task OnGet()
	{
		History = await db.WikiPages
			.ThatAreNotDeleted()
			.CreatedBy(UserName)
			.ByMostRecent()
			.Select(wp => new HistoryEntry(
				wp.Revision, wp.CreateTimestamp, wp.PageName, wp.MinorEdit, wp.RevisionMessage))
			.PageOf(Paging);
	}

	public record HistoryEntry(int Revision, DateTime CreateTimestamp, string PageName, bool MinorEdit, string? RevisionMessage);
}
