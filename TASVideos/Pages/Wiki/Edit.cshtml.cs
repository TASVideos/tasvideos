using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[RequireEdit]
public class EditModel(IWikiPages wikiPages, ApplicationDbContext db, IExternalMediaPublisher publisher) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	[BindProperty]
	public DateTime EditStart { get; set; } = DateTime.UtcNow;

	[BindProperty]
	[DoNotTrim]
	public string Markup { get; set; } = "";

	public string OriginalMarkup => Markup;

	[BindProperty]
	[MaxLength(500)]
	public string? EditComments { get; set; }

	public async Task<IActionResult> OnGet()
	{
		Path = Path?.Trim('/');
		if (string.IsNullOrWhiteSpace(Path))
		{
			return NotFound();
		}

		if (!WikiHelper.IsValidWikiPageName(Path))
		{
			return NotFound();
		}

		if (WikiHelper.IsHomePage(Path))
		{
			var existingUser = await UserName(Path);
			if (string.IsNullOrEmpty(existingUser))
			{
				return NotFound();
			}
		}

		var page = await wikiPages.Page(Path);
		Markup = page?.Markup ?? "";

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		Path = Path?.Trim('/');
		if (string.IsNullOrWhiteSpace(Path))
		{
			return NotFound();
		}

		if (!WikiHelper.IsValidWikiPageName(Path))
		{
			return Home();
		}

		var existingUser = await UserName(Path);
		if (WikiHelper.IsHomePage(Path))
		{
			if (string.IsNullOrEmpty(existingUser))
			{
				return Home();
			}

			Path = Path.Replace(existingUser, existingUser, StringComparison.InvariantCultureIgnoreCase); // Normalize user name
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var page = new WikiCreateRequest
		{
			CreateTimestamp = EditStart,
			PageName = Path.Trim('/'),
			Markup = Markup,
			MinorEdit = HttpContext.Request.MinorEdit(),
			RevisionMessage = EditComments,
			AuthorId = User.GetUserId()
		};
		var result = await wikiPages.Add(page);
		if (result is null)
		{
			ModelState.AddModelError("", "Unable to save. The content on this page may have been modified by another user.");
			return Page();
		}

		await Announce(result, result.Revision == 1);

		return BaseRedirect("/" + page.PageName);
	}

	public async Task<IActionResult> OnPostRollbackLatest()
	{
		var latestRevision = await wikiPages.Page(Path);
		if (latestRevision is null)
		{
			return NotFound();
		}

		if (latestRevision.Revision == 1)
		{
			return BadRequest("Cannot rollback the first revision of a page, just delete instead.");
		}

		var previousRevision = await db.WikiPages
			.Where(wp => wp.PageName == Path)
			.ThatAreNotCurrent()
			.OrderByDescending(wp => wp.Revision)
			.FirstOrDefaultAsync();

		if (previousRevision is null)
		{
			return NotFound();
		}

		var rollBackRevision = new WikiCreateRequest
		{
			PageName = Path!,
			RevisionMessage = $"Rolling back Revision {latestRevision.Revision} \"{latestRevision.RevisionMessage}\"",
			Markup = previousRevision.Markup,
			AuthorId = User.GetUserId(),
			MinorEdit = false
		};

		var result = await wikiPages.Add(rollBackRevision);
		if (result is not null)
		{
			await Announce(result);
		}

		return BasePageRedirect("PageHistory", new { Path, Latest = true });
	}

	private async Task<string?> UserName(string path)
	{
		var userName = WikiHelper.ToUserName(path);
		return await db.Users
			.ForUser(userName)
			.Select(u => u.UserName)
			.SingleOrDefaultAsync();
	}

	private async Task Announce(IWikiPage page, bool force = false)
		=> await publisher.SendWiki(
			$"Page [{Path}]({{0}}) {(page.Revision > 1 ? "edited" : "created")} by {User.Name()}",
			$"{page.RevisionMessage}",
			Path!,
			force);
}
