namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class PageHistoryModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	[FromQuery]
	public int? FromRevision { get; set; }

	[FromQuery]
	public int? ToRevision { get; set; }

	public string PageName { get; set; } = "";

	public List<WikiRevisionModel> Revisions { get; set; } = [];

	public WikiDiffModel Diff { get; set; } = new("", "");

	[FromQuery]
	public bool? Latest { get; set; }

	public async Task OnGet()
	{
		Path = Path?.Trim('/') ?? "";
		PageName = Path;
		Revisions = await db.WikiPages
			.ForPage(Path)
			.ThatAreNotDeleted()
			.OrderBy(wp => wp.Revision)
			.Select(wp => new WikiRevisionModel(
				wp.Revision,
				wp.CreateTimestamp,
				wp.Author!.UserName,
				wp.MinorEdit,
				wp.RevisionMessage))
			.ToListAsync();

		if (Latest == true)
		{
			var (from, to) = await GetLatestRevisions(Path);
			FromRevision = from;
			ToRevision = to;
		}

		if (FromRevision.HasValue && ToRevision.HasValue)
		{
			var diff = await GetPageDiff(Path, FromRevision.Value, ToRevision.Value);
			if (diff is not null)
			{
				Diff = diff;
			}
		}
	}

	private async Task<WikiDiffModel?> GetPageDiff(string pageName, int fromRevision, int toRevision)
	{
		var revisions = await db.WikiPages
			.ForPage(pageName)
			.Where(wp => wp.Revision == fromRevision
				|| wp.Revision == toRevision)
			.ToListAsync();

		if (revisions.Count != (fromRevision == toRevision ? 1 : 2))
		{
			return null;
		}

		return new WikiDiffModel(
			revisions.Single(wp => wp.Revision == fromRevision).Markup,
			revisions.Single(wp => wp.Revision == toRevision).Markup);
	}

	private async Task<(int? From, int? To)> GetLatestRevisions(string pageName)
	{
		var revisions = await db.WikiPages
			.ForPage(pageName)
			.ThatAreNotDeleted()
			.OrderByDescending(wp => wp.Revision)
			.Select(wp => wp.Revision)
			.Take(2)
			.ToListAsync();

		if (!revisions.Any())
		{
			return (null, null);
		}

		// If count is 1, it must be a new page with no history, so compare against nothing
		return revisions.Count == 1
			? (1, 1)
			: (revisions[1], revisions[0]);
	}

	public record WikiDiffModel(string LeftMarkup, string RightMarkup);

	public record WikiRevisionModel(int Revision, DateTime CreateTimestamp, string? CreateUserName, bool MinorEdit, string? RevisionMessage);
}
