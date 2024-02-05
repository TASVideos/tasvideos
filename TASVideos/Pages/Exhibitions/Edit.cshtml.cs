using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Exhibitions.Models;

namespace TASVideos.Pages.Exhibitions
{
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		public EditModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public ExhibitionEditModel Exhibition { get; set; } = new();

		public List<SelectListItem> AvailableGames { get; set; } = new();

		public List<SelectListItem> AvailableUsers { get; set; } = new();

		public async Task<IActionResult> OnGet()
		{
			var exhibition = await _db.Exhibitions
				.Where(e => e.Id == Id)
				.Select(e => new ExhibitionEditModel
				{
					Title = e.Title,
					ExhibitionTimestamp = e.ExhibitionTimestamp.ToUniversalTime().ToString("u", CultureInfo.InvariantCulture),
					Games = e.Games.Select(g => g.Id).ToList(),
					Contributors = e.Contributors.Select(c => c.Id).ToList(),
					Files = e.Files
						.Select(f => new ExhibitionEditModel.ExhibitionFileDisplayModel
						{
							Path = f.Path,
							Type = f.Type,
							Description = f.Description ?? ""
						})
						.ToList(),
					Urls = e.Urls
						.Select(u => new ExhibitionEditModel.ExhibitionUrlDisplayModel
						{
							Url = u.Url,
							Type = u.Type,
							DisplayName = u.DisplayName ?? ""
						}).ToList()
				})
				.SingleOrDefaultAsync();

			if (exhibition is null)
			{
				return NotFound();
			}

			Exhibition = exhibition;

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

			var exhibition = await _db.Exhibitions
				.Include(e => e.Games)
				.Include(e => e.Contributors)
				.SingleOrDefaultAsync(e => e.Id == Id);

			if (exhibition is null)
			{
				return NotFound();
			}

			exhibition.Title = Exhibition.Title;
			exhibition.ExhibitionTimestamp = DateTime.Parse(Exhibition.ExhibitionTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
			exhibition.Games = await _db.Games.Where(g => Exhibition.Games.Contains(g.Id)).ToListAsync();
			exhibition.Contributors = await _db.Users.Where(u => Exhibition.Contributors.Contains(u.Id)).ToListAsync();

			await _db.SaveChangesAsync();

			return RedirectToPage("View", new { Id });
		}
	}
}
