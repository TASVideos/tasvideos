using TASVideos.Common;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationMetaData)]
public class EditModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IWikiPages wikiPages,
	ITagService tagsService,
	IFlagService flagsService,
	IPublicationMaintenanceLogger publicationMaintenanceLogger,
	IYoutubeSync youtubeSync)
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

		await UpdatePublication(Id, Publication);
		return RedirectToPage("View", new { Id });
	}

	public async Task<IActionResult> OnGetTitle(int publicationId)
	{
		var title = (await db.Publications.SingleOrDefaultAsync(p => p.Id == publicationId))?.Title;
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

	private async Task UpdatePublication(int id, PublicationEdit model)
	{
		var externalMessages = new List<string>();

		var publication = await db.Publications
			.Include(p => p.Authors)
			.ThenInclude(pa => pa.Author)
			.Include(p => p.System)
			.Include(p => p.SystemFrameRate)
			.Include(p => p.Game)
			.Include(p => p.GameVersion)
			.Include(p => p.GameGoal)
			.Include(p => p.PublicationUrls)
			.Include(p => p.PublicationTags)
			.Include(p => p.PublicationFlags)
			.SingleOrDefaultAsync(p => p.Id == id);

		if (publication is null)
		{
			return;
		}

		// TODO: this has to be done anytime a string-list TagHelper is used, can we make this automatic with model binders?
		Publication.Authors = Publication.Authors.RemoveEmpty();

		if (publication.ObsoletedById != model.ObsoletedBy)
		{
			externalMessages.Add($"Changed obsoleting movie from \"{publication.ObsoletedById}\" to \"{model.ObsoletedBy}\"");
		}

		if (publication.AdditionalAuthors != model.ExternalAuthors)
		{
			externalMessages.Add($"Changed external authors from \"{publication.AdditionalAuthors}\" to \"{model.ExternalAuthors}\"");
		}

		publication.ObsoletedById = model.ObsoletedBy;
		publication.EmulatorVersion = model.EmulatorVersion;
		publication.AdditionalAuthors = model.ExternalAuthors.NullIfWhitespace();
		publication.Authors.Clear();
		publication.Authors.AddRange(await db.Users
			.ForUsers(Publication.Authors)
			.Select(u => new PublicationAuthor
			{
				PublicationId = publication.Id,
				UserId = u.Id,
				Author = u,
				Ordinal = Publication.Authors.IndexOf(u.UserName)
			})
			.ToListAsync());

		publication.GenerateTitle();

		List<int> editableFlags = await db.Flags
			.Where(f => f.PermissionRestriction.HasValue && User.Permissions().Contains(f.PermissionRestriction.Value) || f.PermissionRestriction == null)
			.Select(f => f.Id)
			.ToListAsync();
		List<PublicationFlag> existingEditablePublicationFlags = publication.PublicationFlags.Where(pf => editableFlags.Contains(pf.FlagId)).ToList();
		List<int> selectedEditableFlagIds = model.SelectedFlags.Intersect(editableFlags).ToList();

		var flagsToAdd = publication.PublicationFlags.Except(existingEditablePublicationFlags).ToList();
		publication.PublicationFlags.Clear();
		publication.PublicationFlags.AddRange(flagsToAdd);
		publication.PublicationFlags.AddFlags(selectedEditableFlagIds);
		externalMessages.AddRange((await flagsService
			.GetDiff(existingEditablePublicationFlags.Select(p => p.FlagId), selectedEditableFlagIds))
			.ToMessages("flags"));

		externalMessages.AddRange((await tagsService
			.GetDiff(publication.PublicationTags.Select(p => p.TagId), model.SelectedTags))
			.ToMessages("tags"));

		publication.PublicationTags.Clear();
		db.PublicationTags.RemoveRange(
			db.PublicationTags.Where(pt => pt.PublicationId == publication.Id));

		publication.PublicationTags.AddTags(model.SelectedTags);

		await db.SaveChangesAsync();
		var existingWikiPage = await wikiPages.PublicationPage(Id);
		IWikiPage? pageToSync = existingWikiPage;

		if (model.Markup != existingWikiPage!.Markup)
		{
			pageToSync = await wikiPages.Add(new WikiCreateRequest
			{
				PageName = WikiHelper.ToPublicationWikiPageName(id),
				Markup = model.Markup,
				MinorEdit = HttpContext.Request.MinorEdit(),
				RevisionMessage = model.RevisionMessage,
				AuthorId = User.GetUserId()
			});
			externalMessages.Add("Description updated");
		}

		foreach (var url in publication.PublicationUrls.ThatAreStreaming())
		{
			if (youtubeSync.IsYoutubeUrl(url.Url))
			{
				await youtubeSync.SyncYouTubeVideo(new YoutubeVideo(
					Id,
					publication.CreateTimestamp,
					url.Url!,
					url.DisplayName,
					publication.Title,
					pageToSync!,
					publication.System!.Code,
					publication.Authors.OrderBy(pa => pa.Ordinal).Select(a => a.Author!.UserName),
					publication.ObsoletedById));
			}
		}

		await publicationMaintenanceLogger.Log(Id, User.GetUserId(), externalMessages);
		await publisher.SendPublicationEdit(User.Name(), Id, $"{string.Join(", ", externalMessages)} | {publication.Title}");
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
