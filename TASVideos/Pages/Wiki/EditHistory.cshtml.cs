using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class EditHistoryModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public EditHistoryModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public string UserName { get; set; } = "";

	public IEnumerable<UserWikiEditHistoryModel> History { get; set; } = new List<UserWikiEditHistoryModel>();

	public async Task OnGet()
	{
		History = await _db.WikiPages
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
