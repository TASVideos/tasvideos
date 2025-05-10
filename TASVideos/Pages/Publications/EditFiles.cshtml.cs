namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class EditFilesModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IMediaFileUploader uploader,
	IPublicationMaintenanceLogger publicationMaintenanceLogger)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public string Title { get; set; } = "";

	public List<PublicationFile> Files { get; set; } = [];

	[Required]
	[BindProperty]
	public IFormFile? NewScreenshot { get; set; }

	[BindProperty]
	[StringLength(250)]
	public string? Description { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var title = await db.Publications
			.Where(p => p.Id == Id)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

		if (title is null)
		{
			return NotFound();
		}

		Title = title;
		Files = await db.PublicationFiles
			.ForPublication(Id)
			.ToListAsync();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			Files = await db.PublicationFiles
				.ForPublication(Id)
				.ToListAsync();

			return Page();
		}

		var path = await uploader.UploadScreenshot(Id, NewScreenshot!, Description);
		await Log($"Added Screenshot file {path}");
		return RedirectToPage("EditFiles", new { Id });
	}

	public async Task<IActionResult> OnPostDelete(int publicationFileId)
	{
		var file = await uploader.DeleteFile(publicationFileId);

		if (file is not null)
		{
			await Log($"Deleted {file.Type} file {file.Path}");
		}

		return RedirectToPage("EditFiles", new { Id });
	}

	private async Task Log(string log)
	{
		SuccessStatusMessage(log);
		await publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
		await publisher.SendPublicationEdit(User.Name(), Id, $"{log} | {Title}");
	}
}
