using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data;
using TASVideos.Data.Entity.Exhibition;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Exhibitions.Models;

namespace TASVideos.Pages.Exhibitions
{
    public class CreateModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		public CreateModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[BindProperty]
		public ExhibitionCreateModel Exhibition { get; set; } = new();

		public List<SelectListItem> AvailableGames { get; set; } = new();

		public List<SelectListItem> AvailableUsers { get; set; } = new();

		public IEnumerable<SelectListItem> AvailableUrlTypes { get; set; } = Enum
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
			await PopulateDropdowns();
			return Page();
		}

		private async Task PopulateDropdowns()
		{
			AvailableGames = await _db.Games
				.OrderBy(g => g.DisplayName)
				.Select(g => new SelectListItem
				{
					Text = g.DisplayName,
					Value = g.Id.ToString()
				})
				.ToListAsync();

			AvailableUsers = await _db.Users
				.OrderBy(u => u.UserName)
				.Select(u => new SelectListItem
				{
					Text = u.UserName,
					Value = u.Id.ToString()
				})
				.ToListAsync();
		}


		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await PopulateDropdowns();
				return Page();
			}

			DateTime exhibitionTimestamp;
			try
			{
				exhibitionTimestamp = DateTime.Parse(Exhibition.ExhibitionTimestamp);
			}
			catch
			{
				ModelState.AddModelError($"{nameof(Exhibition)}.{nameof(Exhibition.ExhibitionTimestamp)}", "Timestamp could not be parsed. Example: 2023-08-21 08:00:00Z");
				await PopulateDropdowns();
				return Page();
			}

			var exhibition = new Exhibition();
			exhibition.Title = Exhibition.Title;
			exhibition.ExhibitionTimestamp = exhibitionTimestamp;
			exhibition.Games = await _db.Games.Where(g => Exhibition.Games.Contains(g.Id)).ToListAsync();
			exhibition.Contributors = await _db.Users.Where(u => Exhibition.Contributors.Contains(u.Id)).ToListAsync();
			foreach (var url in Exhibition.Urls)
			{
				var urlEntity = new ExhibitionUrl();
				urlEntity.Type = url.Type;
				urlEntity.Url = url.Url;
				urlEntity.DisplayName = url.DisplayName;
				exhibition.Urls.Add(urlEntity);
			}

			_db.Exhibitions.Add(exhibition);
			await _db.SaveChangesAsync();

			return BaseRedirect($"/{exhibition.Id}E");
		}
    }
}
