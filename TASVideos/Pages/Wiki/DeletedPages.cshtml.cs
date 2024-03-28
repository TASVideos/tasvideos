using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
public class DeletedPagesModel(IWikiPages wikiPages, ApplicationDbContext db, ExternalMediaPublisher publisher)
	: BasePageModel
{
	public List<DeletedWikiPage> DeletedPages { get; set; } = [];

	public async Task OnGet()
	{
		DeletedPages = await db.WikiPages
			.ThatAreDeleted()
			.GroupBy(tkey => tkey.PageName)
			.Select(record => new DeletedWikiPage(
				record.Key,
				record.Count(),
				db.WikiPages.Any(wp => !wp.IsDeleted && wp.PageName == record.Key)))
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
			var result = await wikiPages.Delete(path);

			if (result == -1)
			{
				ModelState.AddModelError("", "Unable to delete page, the page may have been modified during the saving of this operation.");
				return Page();
			}

			await publisher.SendGeneralWiki(
				$"Page {path} DELETED by {User.Name()}",
				"",
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
		await wikiPages.Delete(path, revision);

		await publisher.SendGeneralWiki(
			$"Revision {revision} of {path} DELETED by {User.Name()}", "", "", "");

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
		var result = await wikiPages.Undelete(path);
		if (!result)
		{
			ModelState.AddModelError("", "Unable to undelete, the page may have been modified during the saving of this operation.");
			return Page();
		}

		await publisher.SendGeneralWiki(
			$"Page {path} UNDELETED by {User.Name()}",
			$"Page [{path}]({{0}}) UNDELETED by {User.Name()}",
			"",
			WikiHelper.EscapeUserName(path));

		return BaseRedirect("/" + path);
	}

	public record DeletedWikiPage(string PageName, int RevisionCount, bool HasExistingRevisions);
}
