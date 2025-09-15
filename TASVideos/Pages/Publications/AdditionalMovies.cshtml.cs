namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.CreateAdditionalMovieFiles)]
public class AdditionalMoviesModel(
	ApplicationDbContext db,
	IPublications publications,
	IExternalMediaPublisher publisher,
	IPublicationMaintenanceLogger publicationMaintenanceLogger,
	IQueueService queueService)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public string PublicationTitle { get; set; } = "";

	public List<FileEntry> AvailableMovieFiles { get; set; } = [];

	[BindProperty]
	[StringLength(50)]
	public string DisplayName { get; set; } = "";

	[Required]
	[BindProperty]
	public IFormFile? AdditionalMovieFile { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var publicationTitle = await publications.GetTitle(Id);
		if (publicationTitle is null)
		{
			return NotFound();
		}

		PublicationTitle = publicationTitle;
		await PopulateAvailableMovieFiles();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var publicationTitle = await publications.GetTitle(Id);
		if (publicationTitle is null)
		{
			return NotFound();
		}

		// Explicitly reject zip files - only individual movie files are allowed
		if (AdditionalMovieFile.IsZip())
		{
			ModelState.AddModelError(nameof(AdditionalMovieFile), "Zip files are not allowed. Please upload the individual movie file instead.");
		}

		AdditionalMovieFile?.AddModelErrorIfOverSizeLimit(ModelState, User);

		if (!ModelState.IsValid)
		{
			PublicationTitle = publicationTitle;
			await PopulateAvailableMovieFiles();
			return Page();
		}

		// Parse the individual movie file (zip files are rejected above)
		var (parseResult, movieFileBytes) = await queueService.ParseMovieFile(AdditionalMovieFile!);
		if (!parseResult.Success)
		{
			ModelState.AddParseErrors(parseResult);
			PublicationTitle = publicationTitle;
			await PopulateAvailableMovieFiles();
			return Page();
		}

		db.PublicationFiles.Add(new PublicationFile
		{
			Path = AdditionalMovieFile!.FileName,
			PublicationId = Id,
			Description = DisplayName,
			Type = FileType.MovieFile,
			FileData = movieFileBytes
		});

		string log = $"Added new movie file: {DisplayName}";
		await publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
		var result = await db.TrySaveChanges();
		SetMessage(result, log, "Unable to add file");
		if (result.IsSuccess())
		{
			await publisher.SendPublicationEdit(User.Name(), Id, $"{log} | {PublicationTitle}");
		}

		return RedirectToPage("AdditionalMovies", new { Id });
	}

	public async Task<IActionResult> OnPostDelete(int publicationFileId)
	{
		var file = await db.PublicationFiles.FindAsync(publicationFileId);

		if (file is not null)
		{
			db.PublicationFiles.Remove(file);

			string log = $"Removed movie file {file.Path}";
			await publicationMaintenanceLogger.Log(file.PublicationId, User.GetUserId(), log);
			var result = await db.TrySaveChanges();
			SetMessage(result, log, "Unable to delete file");
			if (result.IsSuccess())
			{
				await publisher.SendPublicationEdit(User.Name(), Id, $"{log}");
			}
		}

		return RedirectToPage("AdditionalMovies", new { Id });
	}

	private async Task PopulateAvailableMovieFiles()
	{
		AvailableMovieFiles = await db.PublicationFiles
			.ThatAreMovieFiles()
			.ForPublication(Id)
			.Select(pf => new FileEntry(pf.Id, pf.Description, pf.Path))
			.ToListAsync();
	}

	public record FileEntry(int Id, string? Description, string FileName);
}
