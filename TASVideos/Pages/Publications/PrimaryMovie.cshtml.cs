using TASVideos.Core.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.ReplacePrimaryMovieFile)]
public class PrimaryMoviesModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IPublicationMaintenanceLogger publicationMaintenanceLogger)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public string PublicationTitle { get; set; } = "";

	public string OriginalFileName { get; set; } = "";

	[Required]
	[BindProperty]
	[Display(Name = "Primary Movie File", Description = "Your movie packed in a ZIP file (max size: 150k)")]
	public IFormFile? PrimaryMovieFile { get; set; }

	[Required]
	[BindProperty]
	public string? Reason { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var publication = await db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new { p.Title, p.MovieFileName })
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		PublicationTitle = publication.Title;
		OriginalFileName = publication.MovieFileName;
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var publication = await db.Publications
			.Where(p => p.Id == Id)
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		var exists = await db.Publications.AnyAsync(p => p.Id != publication.Id && p.MovieFileName == PrimaryMovieFile!.FileName);
		if (exists)
		{
			ModelState.AddModelError(nameof(PrimaryMovieFile), $"A publication with the filename {PrimaryMovieFile!.FileName} already exists.");
		}

		if (!ModelState.IsValid)
		{
			PublicationTitle = publication.Title;
			OriginalFileName = publication.MovieFileName;
			return Page();
		}

		string log = $"Primary movie file replaced, Reason: {Reason}";
		publication.MovieFileName = PrimaryMovieFile!.FileName;
		publication.MovieFile = await PrimaryMovieFile.ToBytes();

		var result = await ConcurrentSave(db, log, "Unable to add file");
		if (result)
		{
			await publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
			await publisher.SendPublicationEdit(User.Name(), Id, $"{log} | {PublicationTitle}");
		}

		return RedirectToPage("Edit", new { Id });
	}
}
