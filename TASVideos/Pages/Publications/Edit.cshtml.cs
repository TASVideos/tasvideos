using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationMetaData)]
public class EditModel(
	ApplicationDbContext db,
	IPublications publications,
	IExternalMediaPublisher publisher,
	IWikiPages wikiPages,
	IPublicationMaintenanceLogger publicationMaintenanceLogger)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public PublicationEdit Publication { get; set; } = new();

	public List<SelectListItem> AvailableFlags { get; set; } = [];

	public List<SelectListItem> AvailableTags { get; set; } = [];

	public List<PublicationFileDisplay> Files { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var publication = await db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new PublicationEdit
			{
				PublicationClass = p.PublicationClass!.Name,
				MovieFileName = p.MovieFileName,
				ClassIconPath = p.PublicationClass.IconPath,
				ClassLink = p.PublicationClass.Link,
				SystemCode = p.System!.Code,
				Title = p.Title,
				ObsoletedBy = p.ObsoletedById,
				ObsoletedByTitle = p.ObsoletedBy != null ? p.ObsoletedBy.Title : null,
				EmulatorVersion = p.EmulatorVersion,
				ExternalAuthors = p.AdditionalAuthors,
				Urls = p.PublicationUrls
					.Select(u => new PublicationUrlDisplay(
						u.Id, u.Url!, u.Type, u.DisplayName))
					.ToList(),
				SelectedFlags = p.PublicationFlags
					.Select(pf => pf.FlagId)
					.ToList(),
				SelectedTags = p.PublicationTags
					.Select(pt => pt.TagId)
					.ToList()
			})
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		publication.Markup = (await wikiPages.PublicationPage(Id))?.Markup ?? "";

		Publication = publication;
		Publication.Authors = await db.PublicationAuthors
			.Where(pa => pa.PublicationId == Id)
			.OrderBy(pa => pa.Ordinal)
			.Select(pa => pa.Author!.UserName)
			.ToListAsync();

		await PopulateDropdowns();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await PopulateDropdowns();
			return Page();
		}

		if (Publication.ObsoletedBy.HasValue)
		{
			var obsoletedByExists = await db.Publications.AnyAsync(p => p.Id == Publication.ObsoletedBy.Value);
			if (!obsoletedByExists)
			{
				ModelState.AddModelError($"{nameof(Publication)}.{nameof(Publication.ObsoletedBy)}", "Publication does not exist");
				return Page();
			}
		}

		// TODO: this has to be done anytime a string-list TagHelper is used, can we make this automatic with model binders?
		Publication.Authors = Publication.Authors.RemoveEmpty();

		var updateRequest = new UpdatePublicationRequest(
			Publication.ObsoletedBy,
			Publication.EmulatorVersion,
			Publication.ExternalAuthors,
			Publication.Authors,
			Publication.SelectedFlags,
			Publication.SelectedTags,
			User.Permissions().ToList(),
			Publication.Markup,
			Publication.RevisionMessage,
			User.GetUserId(),
			HttpContext.Request.MinorEdit());

		var updateResult = await publications.UpdatePublication(Id, updateRequest);
		if (updateResult.Success)
		{
			await publicationMaintenanceLogger.Log(Id, User.GetUserId(), updateResult.ChangeMessages);
			await publisher.SendPublicationEdit(User.Name(), Id, $"{string.Join(", ", updateResult.ChangeMessages)} | {updateResult.Title}");
		}

		return RedirectToPage("View", new { Id });
	}

	public async Task<IActionResult> OnGetTitle(int publicationId)
	{
		var title = await publications.GetTitle(publicationId);
		return Content(title ?? "");
	}

	private async Task PopulateDropdowns()
	{
		AvailableFlags = await db.Flags.ToDropDownList(User.Permissions());
		AvailableTags = await db.Tags.ToDropdownList();
		Files = await db.PublicationFiles
			.Where(f => f.PublicationId == Id)
			.Select(f => new PublicationFileDisplay(
				f.Id, f.Path, f.Type, f.Description))
			.ToListAsync();
	}

	public class PublicationEdit
	{
		public string SystemCode { get; init; } = "";
		public string Title { get; init; } = "";
		public string MovieFileName { get; init; } = "";
		public string? ExternalAuthors { get; init; }
		public List<string> Authors { get; set; } = [];
		public string PublicationClass { get; init; } = "";
		public string? ClassIconPath { get; init; } = "";
		public string ClassLink { get; init; } = "";
		public int? ObsoletedBy { get; init; }
		public string? ObsoletedByTitle { get; init; }

		[StringLength(50)]
		public string? EmulatorVersion { get; init; }
		public List<int> SelectedFlags { get; init; } = [];
		public List<int> SelectedTags { get; init; } = [];
		public string? RevisionMessage { get; init; }

		[DoNotTrim]
		public string Markup { get; set; } = "";
		public List<PublicationUrlDisplay> Urls { get; init; } = [];
	}

	public record PublicationFileDisplay(int Id, string Path, FileType Type, string? Description);

	public record PublicationUrlDisplay(int Id, string Url, PublicationUrlType Type, string? DisplayName);
}
