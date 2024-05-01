using TASVideos.Core.Services.PublicationChain;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db, IFileService fileService, IPublicationHistory history) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public IndexModel.PublicationDisplay Publication { get; set; } = new();
	public PublicationHistoryGroup History { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var publication = await db.Publications
			.ToViewModel(false, User.GetUserId())
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (publication is null)
		{
			return NotFound();
		}

		Publication = publication;
		History = await history.ForGame(publication.GameId) ?? new();

		ViewData["ReturnUrl"] = HttpContext.CurrentPathToReturnUrl();
		return Page();
	}

	public async Task<IActionResult> OnGetDownload() => ZipFile(await fileService.GetPublicationFile(Id));

	public async Task<IActionResult> OnGetDownloadAdditional(int fileId)
		=> ZipFile(await fileService.GetAdditionalPublicationFile(Id, fileId));
}
