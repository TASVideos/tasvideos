﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationMetaData)]
public class EditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IWikiPages _wikiPages;
	private readonly ExternalMediaPublisher _publisher;
	private readonly ITagService _tagsService;
	private readonly IFlagService _flagsService;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly IYoutubeSync _youtubeSync;

	public EditModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher,
		IWikiPages wikiPages,
		ITagService tagsService,
		IFlagService flagsService,
		IPublicationMaintenanceLogger publicationMaintenanceLogger,
		IYoutubeSync youtubeSync)
	{
		_db = db;
		_wikiPages = wikiPages;
		_publisher = publisher;
		_tagsService = tagsService;
		_flagsService = flagsService;
		_publicationMaintenanceLogger = publicationMaintenanceLogger;
		_youtubeSync = youtubeSync;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public PublicationEditModel Publication { get; set; } = new();

	[Display(Name = "Available Flags")]
	public IEnumerable<SelectListItem> AvailableFlags { get; set; } = new List<SelectListItem>();

	[Display(Name = "Available Tags")]
	public IEnumerable<SelectListItem> AvailableTags { get; set; } = new List<SelectListItem>();

	public IEnumerable<PublicationFileDisplayModel> Files { get; set; } = new List<PublicationFileDisplayModel>();

	public async Task<IActionResult> OnGet()
	{
		var publication = await _db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new PublicationEditModel
			{
				Class = p.PublicationClass!.Name,
				MovieFileName = p.MovieFileName,
				ClassIconPath = p.PublicationClass.IconPath,
				ClassLink = p.PublicationClass.Link,
				SystemCode = p.System!.Code,
				Title = p.Title,
				ObsoletedBy = p.ObsoletedById,
				ObsoletedByTitle = p.ObsoletedBy != null ? p.ObsoletedBy.Title : null,
				EmulatorVersion = p.EmulatorVersion,
				AdditionalAuthors = p.AdditionalAuthors,
				Urls = p.PublicationUrls
					.Select(u => new PublicationUrlDisplayModel
					{
						Id = u.Id,
						Url = u.Url!,
						Type = u.Type,
						DisplayName = u.DisplayName
					})
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

		publication.Markup = (await _wikiPages.PublicationPage(Id))?.Markup ?? "";

		Publication = publication;
		Publication.Authors = await _db.PublicationAuthors
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
			var obsoletedBy = await _db.Publications.SingleOrDefaultAsync(p => p.Id == Publication.ObsoletedBy.Value);
			if (obsoletedBy is null)
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
		var title = (await _db.Publications.SingleOrDefaultAsync(p => p.Id == publicationId))?.Title;
		return new ContentResult { Content = title };
	}

	private async Task PopulateDropdowns()
	{
		AvailableFlags = await _db.Flags
			.ToDropDown(User.Permissions())
			.ToListAsync();
		AvailableTags = await _db.Tags
			.ToDropdown()
			.ToListAsync();
		Files = await _db.PublicationFiles
			.Where(f => f.PublicationId == Id)
			.Select(f => new PublicationFileDisplayModel
			{
				Id = f.Id,
				Path = f.Path,
				Type = f.Type,
				Description = f.Description
			})
			.ToListAsync();
	}

	private async Task UpdatePublication(int id, PublicationEditModel model)
	{
		var externalMessages = new List<string>();

		var publication = await _db.Publications
			.IncludeTitleTables()
			.Include(p => p.PublicationUrls)
			.Include(p => p.PublicationTags)
			.Include(p => p.PublicationFlags)
			.SingleOrDefaultAsync(p => p.Id == id);

		if (publication is null)
		{
			return;
		}

		// TODO: this has to be done anytime a string-list TagHelper is used, can we make this automatic with model binders?
		var pubAuthors = Publication.Authors
			.Where(a => !string.IsNullOrWhiteSpace(a))
			.ToList();
		Publication.Authors = pubAuthors;

		if (publication.ObsoletedById != model.ObsoletedBy)
		{
			externalMessages.Add($"Changed obsoleting movie from \"{publication.ObsoletedById}\" to \"{model.ObsoletedBy}\"");
		}

		if (publication.AdditionalAuthors != model.AdditionalAuthors)
		{
			externalMessages.Add($"Changed external authors from \"{publication.AdditionalAuthors}\" to \"{model.AdditionalAuthors}\"");
		}

		publication.ObsoletedById = model.ObsoletedBy;
		publication.EmulatorVersion = model.EmulatorVersion;
		publication.AdditionalAuthors = model.AdditionalAuthors.NullIfWhitespace();
		publication.Authors.Clear();
		publication.Authors.AddRange(await _db.Users
			.ForUsers(Publication.Authors)
			.Select(u => new PublicationAuthor
			{
				PublicationId = publication.Id,
				UserId = u.Id,
				Author = u,
				Ordinal = pubAuthors.IndexOf(u.UserName)
			})
			.ToListAsync());

		publication.GenerateTitle();

		externalMessages.AddRange((await _flagsService
			.GetDiff(publication.PublicationFlags.Select(p => p.FlagId), model.SelectedFlags))
			.ToMessages("flags"));

		publication.PublicationFlags.Clear();
		_db.PublicationFlags.RemoveRange(
			_db.PublicationFlags.Where(pf => pf.PublicationId == publication.Id));

		publication.PublicationFlags.AddFlags(model.SelectedFlags);

		externalMessages.AddRange((await _tagsService
			.GetDiff(publication.PublicationTags.Select(p => p.TagId), model.SelectedTags))
			.ToMessages("tags"));

		publication.PublicationTags.Clear();
		_db.PublicationTags.RemoveRange(
			_db.PublicationTags.Where(pt => pt.PublicationId == publication.Id));

		publication.PublicationTags.AddTags(model.SelectedTags);

		await _db.SaveChangesAsync();
		var existingWikiPage = await _wikiPages.PublicationPage(Id);
		IWikiPage? pageToSync = existingWikiPage;

		if (model.Markup != existingWikiPage!.Markup)
		{
			pageToSync = await _wikiPages.Add(new WikiCreateRequest
			{
				PageName = WikiHelper.ToPublicationWikiPageName(id),
				Markup = model.Markup,
				MinorEdit = model.MinorEdit,
				RevisionMessage = model.RevisionMessage,
				AuthorId = User.GetUserId()
			});
			externalMessages.Add("Description updated");
		}

		foreach (var url in publication.PublicationUrls.ThatAreStreaming())
		{
			if (_youtubeSync.IsYoutubeUrl(url.Url))
			{
				await _youtubeSync.SyncYouTubeVideo(new YoutubeVideo(
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

		await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), externalMessages);

		if (!model.MinorEdit)
		{
			await _publisher.SendPublicationEdit(
				$"{Id}M edited by {User.Name()}",
				$"[{Id}M]({{0}}) edited by {User.Name()}",
				$"{string.Join(", ", externalMessages)} | {publication.Title}",
				$"{Id}M");
		}
	}
}
