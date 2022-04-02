using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
public class DeletedPagesModel : BasePageModel
{
	private readonly ExternalMediaPublisher _publisher;
	private readonly IWikiPages _wikiPages;

	public DeletedPagesModel(
		ExternalMediaPublisher publisher,
		IWikiPages wikiPages)
	{
		_publisher = publisher;
		_wikiPages = wikiPages;
	}

	public IEnumerable<DeletedWikiPageDisplayModel> DeletedPages { get; set; } = new List<DeletedWikiPageDisplayModel>();

	public async Task OnGet()
	{
		DeletedPages = await _wikiPages.Query
			.ThatAreDeleted()
			.GroupBy(tkey => tkey.PageName)
			.Select(record => new DeletedWikiPageDisplayModel
			{
				PageName = record.Key,
				RevisionCount = record.Count(),
				HasExistingRevisions = _wikiPages.Query.Any(wp => !wp.IsDeleted && wp.PageName == record.Key)
			})
			.ToListAsync();
	}

	public async Task<IActionResult> OnPostDeletePage(string path)
	{
		if (!User.Has(PermissionTo.DeleteWikiPages))
		{
			return AccessDenied();
		}

		if (!string.IsNullOrWhiteSpace(path))
		{
			var result = await _wikiPages.Delete(path);

			if (result == -1)
			{
				ModelState.AddModelError("", "Unable to delete page, the page may have been modified during the saving of this operation.");
				return Page();
			}

			await _publisher.SendGeneralWiki(
				$"Page {path} DELETED by {User.Name()}",
				$"{result} revisions",
				"");
		}

		return BasePageRedirect("DeletedPages");
	}

	public async Task<IActionResult> OnPostDeleteRevision(string path, int revision)
	{
		if (!User.Has(PermissionTo.DeleteWikiPages))
		{
			return AccessDenied();
		}

		if (string.IsNullOrWhiteSpace(path) || revision == 0)
		{
			return Home();
		}

		path = path.Trim('/');
		await _wikiPages.Delete(path, revision);

		await _publisher.SendGeneralWiki(
				$"Revision {revision} of {path} DELETED by {User.Name()}",
				"",
				"");

		return BaseRedirect("/" + path);
	}

	public async Task<IActionResult> OnPostUndelete(string path)
	{
		if (!User.Has(PermissionTo.DeleteWikiPages))
		{
			return AccessDenied();
		}

		if (string.IsNullOrWhiteSpace(path))
		{
			return Home();
		}

		path = path.Trim('/');
		var result = await _wikiPages.Undelete(path);
		if (!result)
		{
			ModelState.AddModelError("", "Unable to undelete, the page may have been modified during the saving of this operation.");
			return Page();
		}

		await _publisher.SendGeneralWiki(
				$"Page {path} UNDELETED by {User.Name()}",
				"",
				path);

		return BaseRedirect("/" + path);
	}
}
