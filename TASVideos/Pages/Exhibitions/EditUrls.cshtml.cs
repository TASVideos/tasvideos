using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Policy;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Pages.Exhibitions.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions
{
	public class EditUrlsModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		public EditUrlsModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public List<EditUrlsUrlModel> Urls { get; set; } = new();

		[BindProperty]
		public EditUrlsUrlModel RelevantUrl { get; set; } = new();

		public IEnumerable<SelectListItem> AvailableTypes = Enum
			.GetValues(typeof(ExhibitionUrlType))
			.Cast<ExhibitionUrlType>()
			.ToList()
			.Select(t => new SelectListItem
			{
				Text = t.ToString(),
				Value = ((int)t).ToString()
			});

		public async Task<IActionResult> OnGet()
        {
			var urls = await _db.Exhibitions
				.Where(e => e.Id == Id)
				.Select(e => e.Urls
					.Select(u => new EditUrlsUrlModel {
						Id = u.Id,
						Url = u.Url,
						Type = u.Type,
						DisplayName = u.DisplayName ?? ""
					}))
				.SingleOrDefaultAsync();

			if (urls is null)
			{
				return NotFound();
			}

			Urls = urls.ToList();

			return Page();
        }

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var exhibition = await _db.Exhibitions
				.Include(e => e.Urls)
				.SingleOrDefaultAsync(e => e.Id == Id);

			if (exhibition is null)
			{
				return NotFound();
			}

			ExhibitionUrl? url = exhibition.Urls.SingleOrDefault(u => u.Id == RelevantUrl.Id);

			if (url is null)
			{
				url = new();
				exhibition.Urls.Add(url);
			}

			url.DisplayName = RelevantUrl.DisplayName;
			url.Url = RelevantUrl.Url;
			url.Type = RelevantUrl.Type;

			await _db.SaveChangesAsync();

			return RedirectToPage("Edit", new { Id });
		}
		public async Task<IActionResult> OnPostDelete(int exhibitionUrlId)
		{
			var url = await _db.ExhibitionUrls
				.SingleOrDefaultAsync(eu => eu.Id == exhibitionUrlId);

			if (url != null)
			{
				_db.ExhibitionUrls.Remove(url);
				await _db.SaveChangesAsync();
			}

			return RedirectToPage("Edit", new { Id });
		}
	}
}
