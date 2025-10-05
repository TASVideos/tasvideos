namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.CreateAdditionalMovieFiles)]
public class AdditionalMoviesModel(
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

		var result = await publications.AddMovieFile(Id, AdditionalMovieFile!.FileName, DisplayName, movieFileBytes);
		if (result.IsSuccess())
		{
			string log = $"Added new movie file: {DisplayName}";
			await publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
			await publisher.SendPublicationEdit(User.Name(), Id, $"{log} | {PublicationTitle}");
			SuccessStatusMessage(log);
		}
		else
		{
			ErrorStatusMessage("Unable to add file");
		}

		return RedirectToPage("AdditionalMovies", new { Id });
	}

	public async Task<IActionResult> OnPostDelete(int publicationFileId)
	{
		var (file, result) = await publications.RemoveFile(publicationFileId);

		if (file is null)
		{
			ErrorStatusMessage($"Unable to find file with id {publicationFileId}");
			return RedirectToPage("AdditionalMovies", new { Id });
		}

		if (result.IsSuccess())
		{
			var log = $"Removed movie file {file.Path}";
			SuccessStatusMessage(log);
			await publicationMaintenanceLogger.Log(file.PublicationId, User.GetUserId(), log);
			await publisher.SendPublicationEdit(User.Name(), Id, $"{log}");
		}
		else
		{
			ErrorStatusMessage("Unable to delete file");
		}

		return RedirectToPage("AdditionalMovies", new { Id });
	}

	private async Task PopulateAvailableMovieFiles()
		=> AvailableMovieFiles = await publications.GetAvailableMovieFiles(Id);
}
