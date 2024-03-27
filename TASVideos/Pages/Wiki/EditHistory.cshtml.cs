namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class EditHistoryModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public string UserName { get; set; } = "";

	public List<HistoryEntry> History { get; set; } = [];

	public async Task OnGet()
	{
		History = await db.WikiPages
			.ThatAreNotDeleted()
			.CreatedBy(UserName)
			.ByMostRecent()
			.Select(wp => new HistoryEntry(
				wp.Revision, wp.CreateTimestamp, wp.PageName, wp.MinorEdit, wp.RevisionMessage))
			.ToListAsync();
	}

	public record HistoryEntry(int Revision, DateTime CreateTimestamp, string PageName, bool MinorEdit, string? RevisionMessage);
}
