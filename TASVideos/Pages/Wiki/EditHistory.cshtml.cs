using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class EditHistoryModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public string UserName { get; set; } = "";

	public List<UserWikiEditHistoryModel> History { get; set; } = [];

	public async Task OnGet()
	{
		History = await db.WikiPages
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

	public class UserWikiEditHistoryModel
	{
		[Display(Name = "Revision")]
		public int Revision { get; set; }

		[Display(Name = "Date")]
		public DateTime CreateTimestamp { get; set; }

		[Display(Name = "Page")]
		public string PageName { get; set; } = "";

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		[Display(Name = "Revision Message")]
		public string? RevisionMessage { get; set; }
	}
}
