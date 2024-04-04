using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[RequireEdit]
public class EditModel(IWikiPages wikiPages, ApplicationDbContext db, ExternalMediaPublisher publisher) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	[BindProperty]
	public WikiEditModel PageToEdit { get; set; } = new();

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

		if (WikiHelper.IsHomePage(Path) && !await UserNameExists(Path))
		{
			return NotFound();
		}

		var page = await wikiPages.Page(Path);

		PageToEdit = new WikiEditModel
		{
			Markup = page?.Markup ?? ""
		};

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

		if (WikiHelper.IsHomePage(Path) && !await UserNameExists(Path))
		{
			return Home();
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var page = new WikiCreateRequest
		{
			CreateTimestamp = PageToEdit.EditStart,
			PageName = Path.Trim('/'),
			Markup = PageToEdit.Markup,
			MinorEdit = PageToEdit.MinorEdit,
			RevisionMessage = PageToEdit.RevisionMessage,
			AuthorId = User.GetUserId()
		};
		var result = await wikiPages.Add(page);
		if (result is null)
		{
			ModelState.AddModelError("", "Unable to save. The content on this page may have been modified by another user.");
			return Page();
		}

		if (result.Revision == 1 || !PageToEdit.MinorEdit)
		{
			await Announce(result);
		}

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

	private async Task<bool> UserNameExists(string path)
	{
		var userName = WikiHelper.ToUserName(path);
		return await db.Users.Exists(userName);
	}

	private async Task Announce(IWikiPage page)
	{
		await publisher.SendGeneralWiki(
			$"Page {Path} {(page.Revision > 1 ? "edited" : "created")} by {User.Name()}",
			$"Page [{Path}]({{0}}) {(page.Revision > 1 ? "edited" : "created")} by {User.Name()}",
			$"{page.RevisionMessage}",
			WikiHelper.EscapeUserName(Path!));
	}

	public class WikiEditModel
	{
		public DateTime EditStart { get; init; } = DateTime.UtcNow;

		[DoNotTrim]
		public string Markup { get; init; } = "";

		public string OriginalMarkup => Markup;

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; init; }

		[Display(Name = "Edit Comments", Description = "Please enter a descriptive summary of your change. Leaving this blank is discouraged.")]
		[MaxLength(500)]
		public string? RevisionMessage { get; init; }
	}
}
