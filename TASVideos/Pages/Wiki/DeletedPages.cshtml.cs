using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
public class DeletedPagesModel(IWikiPages wikiPages, ApplicationDbContext db, IExternalMediaPublisher publisher)
	: BasePageModel
{
	public List<DeletedPage> DeletedPages { get; set; } = [];

	public async Task OnGet()
	{
		DeletedPages = await db.WikiPages
			.ThatAreDeleted()
			.GroupBy(tkey => tkey.PageName)
			.Select(record => new DeletedPage(
				record.Key,
				record.Count(),
				db.WikiPages.Any(wp => !wp.IsDeleted && wp.PageName == record.Key)))
			.ToListAsync();
	}

	public async Task<IActionResult> OnPostDeletePage(string path, string reason)
	{
		if (!User.Has(PermissionTo.DeleteWikiPages))
		{
			return AccessDenied();
		}

		if (string.IsNullOrWhiteSpace(path))
		{
			ModelState.AddModelError(nameof(path), "Page not found.");
			return Page();
		}

		var page = await wikiPages.Page(path);
		if (page is null)
		{
			ModelState.AddModelError(nameof(path), "Page not found.");
			return Page();
		}

		if (string.IsNullOrWhiteSpace(reason))
		{
			reason = "(No Reason provided)";
		}

		await wikiPages.Add(new WikiCreateRequest
		{
			PageName = page.PageName,
			Markup = page.Markup,
			RevisionMessage = $"Page deleted, reason: {reason}",
			MinorEdit = true,
			AuthorId = User.GetUserId()
		});

		var result = await wikiPages.Delete(path);

		if (result == -1)
		{
			ModelState.AddModelError("", "Unable to delete page, the page may have been modified during the saving of this operation.");
			return Page();
		}

		await publisher.SendMessage(PostGroups.Wiki, $"Page {path} DELETED by {User.Name()} ({result} revisions\")", reason);

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
		await publisher.SendMessage(PostGroups.Wiki, $"Revision {revision} of {path} DELETED by {User.Name()}");

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

		await publisher.SendWiki(
			$"Page [{path}]({{0}}) UNDELETED by {User.Name()}", "", path);

		return BaseRedirect("/" + path);
	}

	public record DeletedPage(string PageName, int RevisionCount, bool HasExistingRevisions);
}
