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

	[FromQuery]
	public bool? Latest { get; set; }

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

	public async Task<IActionResult> OnPostRollbackLatest(string path)
	{
		// TODO: it is more complex than this
		if (!User.Has(PermissionTo.EditWikiPages))
		{
			return AccessDenied();
		}

		var latestRevision = await _wikiPages.Page(path);
		if (latestRevision is null)
		{
			return NotFound();
		}

		if (latestRevision.Revision == 1)
		{
			return BadRequest("Cannot rollback the first revision of a page, just delete instead.");
		}

		var previousRevision = await _wikiPages.Query
			.Where(wp => wp.PageName == path)
			.ThatAreNotCurrent()
			.OrderByDescending(wp => wp.Revision)
			.FirstOrDefaultAsync();

		if (previousRevision is null)
		{
			return NotFound();
		}

		var rollBackRevision = new WikiPage
		{
			PageName = path,
			RevisionMessage = $"Rolling back Revision {latestRevision.Revision} \"{latestRevision.RevisionMessage}\"",
			Markup = previousRevision.Markup,
			AuthorId = User.GetUserId(),
			MinorEdit = false
		};

		await _wikiPages.Add(rollBackRevision);

		// TOOD: announce

		return BasePageRedirect("PageHistory", new { Path = path, Latest = true });
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
			LeftMarkup = revisions.Single(wp => wp.Revision == fromRevision).Markup,
			RightMarkup = revisions.Single(wp => wp.Revision == toRevision).Markup
		};
	}

	private async Task<(int? from, int? to)> GetLatestRevisions(string pageName)
	{
		var revisions = await _wikiPages.Query
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
}
