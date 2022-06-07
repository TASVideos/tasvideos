using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class EditHistoryModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;

	public EditHistoryModel(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	[FromRoute]
	public string UserName { get; set; } = "";

	public IEnumerable<UserWikiEditHistoryModel> History { get; set; } = new List<UserWikiEditHistoryModel>();

	public async Task OnGet()
	{
		History = await _wikiPages.Query
			.ThatAreNotDeleted()
			.CreatedBy(UserName)
			.ByMostRecent()
			.Select(wp => new UserWikiEditHistoryModel
			{
				Revision = wp.Revision,
				CreateTimestamp = wp.CreateTimestamp,
				PageName = wp.PageName,
				MinorEdit = wp.MinorEdit,
				RevisionMessage = wp.RevisionMessage
			})
			.ToListAsync();
	}
}
