using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

		private static readonly List<PublicationUrlType> PublicationUrlTypes = Enum
			.GetValues(typeof(PublicationUrlType))
			.Cast<PublicationUrlType>()
			.ToList();

		public EditUrlsModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IYoutubeSync youtubeSync)
		{
			_db = db;
			_publisher = publisher;
			_youtubeSync = youtubeSync;
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
			CurrentUrls = await _db.PublicationUrls
				.Where(u => u.PublicationId == Id)
				.ToListAsync();

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

			if (UrlType == PublicationUrlType.Streaming && _youtubeSync.IsYoutubeUrl(PublicationUrl))
			{
				await _youtubeSync.SyncYouTubeVideos(PublicationUrl);
			}

			await _db.SaveChangesAsync();

			_publisher.SendPublicationEdit(
				$"Publication {Id} {Title} added {UrlType} url {PublicationUrl}",
				$"{Id}M",
				User.Name());

			return RedirectToPage("EditUrls", new { Id });
		}

		public async Task<IActionResult> OnPostDelete(int publicationUrlId)
		{
			// TODO: youtube sync logic
			var url = await _db.PublicationUrls
				.SingleOrDefaultAsync(pf => pf.Id == publicationUrlId);

			_db.PublicationUrls.Remove(url);

			_publisher.SendPublicationEdit(
				$"Publication {Id} deleted {url.Type} url {url.Url}",
				$"{Id}M",
				User.Name());

			await _db.SaveChangesAsync();

			return RedirectToPage("EditUrls", new { Id });
		}
	}
}
