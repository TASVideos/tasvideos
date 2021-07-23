using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationFiles)]
	public class EditUrlsModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IYoutubeSync _youtubeSync;
		private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;

		private static readonly List<PublicationUrlType> PublicationUrlTypes = Enum
			.GetValues(typeof(PublicationUrlType))
			.Cast<PublicationUrlType>()
			.ToList();

		public EditUrlsModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IYoutubeSync youtubeSync,
			IPublicationMaintenanceLogger publicationMaintenanceLogger)
		{
			_db = db;
			_publisher = publisher;
			_youtubeSync = youtubeSync;
			_publicationMaintenanceLogger = publicationMaintenanceLogger;
		}

		public IEnumerable<SelectListItem> AvailableTypes =
			PublicationUrlTypes
				.Select(t => new SelectListItem
				{
					Text = t.ToString(),
					Value = ((int)t).ToString()
				});

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public string Title { get; set; } = "";

		public ICollection<PublicationUrl> CurrentUrls { get; set; } = new List<PublicationUrl>();

		[Required]
		[BindProperty]
		[Url]
		[Display(Name = "Url")]
		public string PublicationUrl { get; set; } = "";

		[Required]
		[BindProperty]
		[Display(Name = "Type")]
		public PublicationUrlType UrlType { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Title = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => p.Title)
				.SingleOrDefaultAsync();

			if (Title == null)
			{
				return NotFound();
			}

			CurrentUrls = await _db.PublicationUrls
				.Where(u => u.PublicationId == Id)
				.ToListAsync();

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			var publication = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => new
				{
					Title,
					p.PublicationUrls,
					SystemCode = p.System!.Code,
					p.WikiContent!.Markup,
					Authors = p.Authors.Select(pa => pa.Author!.UserName),
					p.Game!.SearchKey
				})
				.SingleOrDefaultAsync();

			if (publication == null)
			{
				return NotFound();
			}

			CurrentUrls = publication.PublicationUrls;

			if (CurrentUrls.Any(u => u.Type == UrlType && u.Url == PublicationUrl))
			{
				ModelState.AddModelError($"{nameof(PublicationUrl)}", $"The {UrlType} url: {PublicationUrl} already exists");
			}

			if (!ModelState.IsValid)
			{
				return Page();
			}

			var publicationUrl = new PublicationUrl
			{
				PublicationId = Id,
				Url = PublicationUrl,
				Type = UrlType
			};

			_db.PublicationUrls.Add(publicationUrl);

			string log = $"added {UrlType} url {PublicationUrl}";
			await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
			var result = await ConcurrentSave(_db, log, "Unable to add url.");
			if (result)
			{
				_publisher.SendPublicationEdit(
					$"Publication {Id} {Title} added {UrlType} url {PublicationUrl}",
					$"{Id}M",
					User.Name());

				// TODO: render markup, in a youtube friendly way
				if (UrlType == PublicationUrlType.Streaming && _youtubeSync.IsYoutubeUrl(PublicationUrl))
				{
					YoutubeVideo video = new (
						PublicationUrl,
						publication.Title,
						publication.Markup,
						publication.SystemCode,
						publication.Authors,
						publication.SearchKey);
					await _youtubeSync.SyncYouTubeVideo(video);
				}
			}

			return RedirectToPage("EditUrls", new { Id });
		}

		public async Task<IActionResult> OnPostDelete(int publicationUrlId)
		{
			var url = await _db.PublicationUrls
				.SingleOrDefaultAsync(pf => pf.Id == publicationUrlId);

			if (url != null)
			{
				_db.PublicationUrls.Remove(url);
				string log = $"deleted {url.Type} url {url.Url}";
				await _publicationMaintenanceLogger.Log(url.PublicationId, User.GetUserId(), log);
				var result = await ConcurrentSave(_db, log, "Unable to remove url.");
				if (result)
				{
					_publisher.SendPublicationEdit(
						$"Publication {Id} {log}",
						$"{Id}M",
						User.Name());

					await _youtubeSync.UnlistVideo(url.Url!);
				}
			}

			return RedirectToPage("EditUrls", new { Id });
		}
	}
}
