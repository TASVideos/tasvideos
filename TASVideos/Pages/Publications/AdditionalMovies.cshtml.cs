using TASVideos.MovieParsers;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.CreateAdditionalMovieFiles)]
public class AdditionalMoviesModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IPublicationMaintenanceLogger publicationMaintenanceLogger,
	IMovieParser parser)
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
		var publicationTitle = await db.Publications
			.Where(p => p.Id == Id)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

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
		var publication = await db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new { p.Id, p.Title })
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		if (!AdditionalMovieFile.IsZip())
		{
			ModelState.AddModelError(nameof(AdditionalMovieFile), "Not a valid .zip file");
		}

		AdditionalMovieFile?.AddModelErrorIfOverSizeLimit(ModelState, User);

		if (!ModelState.IsValid)
		{
			PublicationTitle = publication.Title;
			await PopulateAvailableMovieFiles();
			return Page();
		}

		var parseResult = await parser.ParseZip(AdditionalMovieFile!.OpenReadStream());
		if (!parseResult.Success)
		{
			ModelState.AddParseErrors(parseResult);
			PublicationTitle = publication.Title;
			await PopulateAvailableMovieFiles();
			return Page();
		}

		db.PublicationFiles.Add(new PublicationFile
		{
			Path = AdditionalMovieFile!.FileName,
			PublicationId = Id,
			Description = DisplayName,
			Type = FileType.MovieFile,
			FileData = await AdditionalMovieFile.ToBytes()
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
