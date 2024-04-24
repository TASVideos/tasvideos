using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.MovieParsers;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.CreateAdditionalMovieFiles)]
public class AdditionalMoviesModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
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
	[Display(Name = "Display Name")]
	public string DisplayName { get; set; } = "";

	[Required]
	[BindProperty]
	[Display(Name = "Add an additional movie file:", Description = "Your movie packed in a ZIP file (max size: 150k)")]
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

		if (!AdditionalMovieFile.LessThanMovieSizeLimit())
		{
			ModelState.AddModelError(
				nameof(AdditionalMovieFile),
				".zip is too big, are you sure this is a valid movie file?");
		}

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

		var publicationFile = new PublicationFile
		{
			Path = AdditionalMovieFile!.FileName,
			PublicationId = Id,
			Description = DisplayName,
			Type = FileType.MovieFile,
			FileData = await AdditionalMovieFile.ToBytes()
		};

		db.PublicationFiles.Add(publicationFile);

		string log = $"Added new movie file: {DisplayName}";
		await publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
		var result = await ConcurrentSave(db, log, "Unable to add file");
		if (result)
		{
			await publisher.SendPublicationEdit(User.Name(), Id, $"{log} | {PublicationTitle}");
		}

		return RedirectToPage("AdditionalMovies", new { Id });
	}

	public async Task<IActionResult> OnPostDelete(int publicationFileId)
	{
		var file = await db.PublicationFiles
			.SingleOrDefaultAsync(pf => pf.Id == publicationFileId);

		if (file is not null)
		{
			db.PublicationFiles.Remove(file);

			string log = $"Removed movie file {file.Path}";
			await publicationMaintenanceLogger.Log(file.PublicationId, User.GetUserId(), log);
			var result = await ConcurrentSave(db, log, "Unable to delete file");

			if (result)
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
