using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	private static readonly List<SelectListItem> Statuses = [.. Enum.GetValues(typeof(SubmissionStatus))
		.Cast<SubmissionStatus>()
		.ToDropDown()
		.OrderBy(s => s.Text)];

	// For legacy routes such as Subs-Rej-422up
	[FromRoute]
	public string? Query { get; set; }

	[FromQuery]
	public SubmissionSearchRequest Search { get; set; } = new();

	public SubmissionPageOf<SubmissionListEntry> Submissions { get; set; } = SubmissionPageOf<SubmissionListEntry>.Empty();

	[Display(Name = "Statuses")]
	public List<SelectListItem> AvailableStatuses => Statuses;

	public List<SelectListItem> SystemList { get; set; } = [];

	public async Task OnGet()
	{
		SystemList = (await db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropDown()
			.ToListAsync())
			.WithDefaultEntry();

		var search = LegacySubListConverter.ToSearchRequest(Query);
		if (search is not null)
		{
			Search = search;
		}

		// Defaults
		// Note that we do not provide these for GameId, the assumption is that we want to see all submissions of a given game, not just active ones
		if (!Search.StatusFilter.Any() && string.IsNullOrWhiteSpace(Search.GameId))
		{
			Search.StatusFilter = !string.IsNullOrWhiteSpace(Search.User) || Search.Years.Any()
				? SubmissionSearchRequest.All
				: SubmissionSearchRequest.Default;
		}

		var entries = await db.Submissions
			.FilterBy(Search)
			.ToSubListEntry()
			.SortedPageOf(Search);

		Submissions = new SubmissionPageOf<SubmissionListEntry>(entries)
		{
			PageSize = entries.PageSize,
			CurrentPage = entries.CurrentPage,
			RowCount = entries.RowCount,
			Sort = entries.Sort,
			Years = Search.Years,
			StatusFilter = Search.StatusFilter,
			System = Search.System,
			GameId = Search.GameId,
			User = Search.User
		};
	}
}
