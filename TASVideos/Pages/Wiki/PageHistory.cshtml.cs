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

	public WikiHistoryModel History { get; set; } = new();

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
	}
}
