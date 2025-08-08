using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[RequireEdit]
public class EditModel(IWikiPages wikiPages, IUserManager userManager, IExternalMediaPublisher publisher) : BasePageModel
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
		if (string.IsNullOrWhiteSpace(Path) || !WikiHelper.IsValidWikiPageName(Path))
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
		if (string.IsNullOrWhiteSpace(Path))
		{
			return NotFound();
		}

		var result = await wikiPages.RollbackLatest(Path, User.GetUserId());
		if (result is null)
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

			return NotFound();
		}

		await Announce(result);
		return BasePageRedirect("PageHistory", new { Path, Latest = true });
	}

	private async Task<string?> UserName(string path)
		=> await userManager.GetUserNameByUserName(
			WikiHelper.ToUserName(path));

	private async Task Announce(IWikiPage page, bool force = false)
		=> await publisher.SendWiki(
			$"Page [{Path}]({{0}}) {(page.Revision > 1 ? "edited" : "created")} by {User.Name()}",
			$"{page.RevisionMessage}",
			Path!,
			force);
}
