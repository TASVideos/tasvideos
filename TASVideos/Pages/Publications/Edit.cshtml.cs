using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationMetaData)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IWikiPages _wikiPages;
		private readonly ExternalMediaPublisher _publisher;
		private readonly ITagService _tagsService;
		private readonly IFlagService _flagsService;
		private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
		private readonly IYoutubeSync _youtubeSync;

		public EditModel(
			ApplicationDbContext db,
			IMapper mapper,
			ExternalMediaPublisher publisher,
			IWikiPages wikiPages,
			ITagService tagsService,
			IFlagService flagsService,
			IPublicationMaintenanceLogger publicationMaintenanceLogger,
			IYoutubeSync youtubeSync)
		{
			_db = db;
			_mapper = mapper;
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
		public PublicationEditModel Publication { get; set; } = new ();

		[Display(Name = "Available Flags")]
		public IEnumerable<SelectListItem> AvailableFlags { get; set; } = new List<SelectListItem>();

		[Display(Name = "Available Tags")]
		public IEnumerable<SelectListItem> AvailableTags { get; set; } = new List<SelectListItem>();

		public IEnumerable<SelectListItem> AvailableMoviesForObsoletedBy { get; set; } = new List<SelectListItem>();

		public IEnumerable<PublicationFileDisplayModel> Files { get; set; } = new List<PublicationFileDisplayModel>();

		public async Task<IActionResult> OnGet()
		{
			Publication = await _db.Publications
					.Where(p => p.Id == Id)
					.Select(p => new PublicationEditModel
					{
						Tier = p.Tier!.Name,
						MovieFileName = p.MovieFileName,
						TierIconPath = p.Tier.IconPath,
						TierLink = p.Tier.Link,
						SystemCode = p.System!.Code,
						Title = p.Title,
						ObsoletedBy = p.ObsoletedById,
						Branch = p.Branch,
						EmulatorVersion = p.EmulatorVersion,
						AdditionalAuthors = p.AdditionalAuthors,
						Urls = p.PublicationUrls
							.Select(u => new PublicationUrlDisplayModel
							{
								Id = u.Id,
								Url = u.Url!,
								Type = u.Type
							})
							.ToList(),
						SelectedFlags = p.PublicationFlags
							.Select(pf => pf.FlagId)
							.ToList(),
						SelectedTags = p.PublicationTags
							.Select(pt => pt.TagId)
							.ToList(),
						Markup = p.WikiContent != null ? p.WikiContent.Markup : ""
					})
					.SingleOrDefaultAsync();

			if (Publication == null)
			{
				return NotFound();
			}

			Publication.Authors = await _db.PublicationAuthors
				.Where(pa => pa.PublicationId == Id)
				.Select(pa => pa.Author!.UserName)
				.ToListAsync();

			await PopulateDropdowns(Publication.SystemCode);
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await PopulateDropdowns(Publication.SystemCode);
				return Page();
			}

			await UpdatePublication(Id, Publication);
			return RedirectToPage("View", new { Id });
		}

		private async Task PopulateDropdowns(string systemCode)
		{
			var userPermissions = User.Permissions();
			AvailableFlags = await _db.Flags
				.Select(f => new SelectListItem
				{
					Text = f.Name,
					Value = f.Id.ToString(),
					Disabled = f.PermissionRestriction.HasValue
						&& !userPermissions.Contains(f.PermissionRestriction.Value)
				})
				.ToListAsync();
			AvailableTags = await _db.Tags
				.Select(f => new SelectListItem
				{
					Text = f.DisplayName,
					Value = f.Id.ToString()
				})
				.ToListAsync();
			AvailableMoviesForObsoletedBy = await _db.Publications
				.ThatAreCurrent()
				.Where(p => p.System!.Code == systemCode)
				.Where(p => p.Id != Id)
				.Select(p => new SelectListItem
				{
					Text = p.Title,
					Value = p.Id.ToString()
				})
				.ToListAsync();
			Files = await _mapper.ProjectTo<PublicationFileDisplayModel>(
					_db.PublicationFiles.Where(f => f.PublicationId == Id))
				.ToListAsync();
		}

		private async Task UpdatePublication(int id, PublicationEditModel model)
		{
			var externalMessages = new List<string>();

			var publication = await _db.Publications
				.Include(p => p.PublicationUrls)
				.Include(p => p.PublicationTags)
				.Include(p => p.PublicationFlags)
				.Include(p => p.WikiContent)
				.Include(p => p.System)
				.Include(p => p.SystemFrameRate)
				.Include(p => p.Game)
				.Include(p => p.Authors)
				.ThenInclude(pa => pa.Author)
				.SingleOrDefaultAsync(p => p.Id == id);

			if (publication is null)
			{
				return;
			}

			// TODO: this has to be done anytime a string-list TagHelper is used, can we make this automatic with model binders?
			Publication.Authors = Publication.Authors
				.Where(a => !string.IsNullOrWhiteSpace(a))
				.ToList();

			if (publication.Branch != model.Branch)
			{
				externalMessages.Add($"Changed branch from \"{publication.Branch}\" to \"{model.Branch}\"");
			}

			publication.Branch = model.Branch;

			if (publication.ObsoletedById != model.ObsoletedBy)
			{
				externalMessages.Add($"Changed obsoleting movie from \"{publication.ObsoletedById}\" to \"{model.ObsoletedBy}\"");
			}

			if (publication.AdditionalAuthors != model.AdditionalAuthors)
			{
				externalMessages.Add($"Changed additional authors from \"{publication.AdditionalAuthors}\" to \"{model.AdditionalAuthors}\"");
			}

			publication.ObsoletedById = model.ObsoletedBy;
			publication.EmulatorVersion = model.EmulatorVersion;
			publication.AdditionalAuthors = string.IsNullOrWhiteSpace(model.AdditionalAuthors)
				? null
				: model.AdditionalAuthors;

			publication.Authors.Clear();
			publication.Authors.AddRange(await _db.Users
				.Where(u => Publication.Authors.Contains(u.UserName))
				.Select(u => new PublicationAuthor
				{
					PublicationId = publication.Id,
					UserId = u.Id,
					Author = u
				})
				.ToListAsync());

			publication.GenerateTitle();

			externalMessages.AddRange((await _flagsService
				.GetDiff(publication.PublicationFlags.Select(p => p.FlagId), model.SelectedFlags))
				.ToMessages("flags"));

			publication.PublicationFlags.Clear();
			_db.PublicationFlags.RemoveRange(
				_db.PublicationFlags.Where(pf => pf.PublicationId == publication.Id));

			foreach (var flag in model.SelectedFlags)
			{
				publication.PublicationFlags.Add(new PublicationFlag
				{
					PublicationId = publication.Id,
					FlagId = flag
				});
			}

			externalMessages.AddRange((await _tagsService
				.GetDiff(publication.PublicationTags.Select(p => p.TagId), model.SelectedTags))
				.ToMessages("tags"));

			publication.PublicationTags.Clear();
			_db.PublicationTags.RemoveRange(
				_db.PublicationTags.Where(pt => pt.PublicationId == publication.Id));

			foreach (var tag in model.SelectedTags)
			{
				publication.PublicationTags.Add(new PublicationTag
				{
					PublicationId = publication.Id,
					TagId = tag
				});
			}

			await _db.SaveChangesAsync();

			if (model.Markup != publication.WikiContent!.Markup)
			{
				var revision = new WikiPage
				{
					PageName = $"{LinkConstants.PublicationWikiPage}{id}",
					Markup = model.Markup,
					MinorEdit = model.MinorEdit,
					RevisionMessage = model.RevisionMessage
				};

				await _wikiPages.Add(revision);
				publication.WikiContentId = revision.Id;
				externalMessages.Add("Description updated");
			}

			foreach (var url in publication.PublicationUrls)
			{
				if (url.Type == PublicationUrlType.Streaming && _youtubeSync.IsYoutubeUrl(url.Url))
				{
					await _youtubeSync.SyncYouTubeVideo(new YoutubeVideo(
						Id,
						publication.CreateTimestamp,
						url.Url!,
						publication.Title,
						publication.WikiContent,
						publication.System!.Code,
						publication.Authors.Select(a => a.Author!.UserName),
						publication.Game!.SearchKey,
						publication.ObsoletedById));
				}
			}

			await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), externalMessages);
			foreach (var message in externalMessages)
			{
				_publisher.SendPublicationEdit($"{publication.Title} edited: " + message, $"{Id}M", User.Name());
			}
		}
	}
}
