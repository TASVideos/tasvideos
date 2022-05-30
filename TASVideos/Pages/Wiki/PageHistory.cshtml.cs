using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class PageHistoryModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;

	public PageHistoryModel(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	[FromQuery]
	public string? Path { get; set; }

	[FromQuery]
	public int? FromRevision { get; set; }

	[FromQuery]
	public int? ToRevision { get; set; }

	public WikiHistoryModel History { get; set; } = new();

	public WikiDiffModel Diff { get; set; } = new();

	public async Task OnGet()
	{
		Path = Path?.Trim('/') ?? "";
		History = new WikiHistoryModel
		{
			PageName = Path,
			Revisions = await _wikiPages.Query
				.ForPage(Path)
				.ThatAreNotDeleted()
				.OrderBy(wp => wp.Revision)
				.Select(wp => new WikiHistoryModel.WikiRevisionModel
				{
					Revision = wp.Revision,
					CreateTimestamp = wp.CreateTimestamp,
					CreateUserName = wp.Author!.UserName,
					MinorEdit = wp.MinorEdit,
					RevisionMessage = wp.RevisionMessage
				})
				.ToListAsync()
		};

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
		var revisions = await _wikiPages.Query
			.ForPage(pageName)
			.Where(wp => wp.Revision == fromRevision
				|| wp.Revision == toRevision)
			.ToListAsync();

		if (revisions.Count != (fromRevision == toRevision ? 1 : 2))
		{
			return null;
		}

		return new WikiDiffModel
		{
			PageName = pageName,
			LeftRevision = fromRevision,
			RightRevision = toRevision,
			LeftMarkup = revisions.Single(wp => wp.Revision == fromRevision).Markup,
			RightMarkup = revisions.Single(wp => wp.Revision == toRevision).Markup
		};
	}
}
