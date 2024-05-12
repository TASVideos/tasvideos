namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db, IFileService fileService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public IndexModel.PublicationDisplay Publication { get; set; } = new();

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
		return Page();
	}

	public async Task<IActionResult> OnGetDownload() => ZipFile(await fileService.GetPublicationFile(Id));

	public async Task<IActionResult> OnGetDownloadAdditional(int fileId)
		=> ZipFile(await fileService.GetAdditionalPublicationFile(Id, fileId));
}
