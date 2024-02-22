using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Pages.Exhibitions.Models;

namespace TASVideos.Pages.Exhibitions
{
    public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public ViewModel(ApplicationDbContext db)
		{
			_db = db;
		}
		[FromRoute]
		public int Id { get; set; }
		public ExhibitionDisplayModel Exhibition { get; set; } = new();
		public async Task<IActionResult> OnGet()
        {
			var exhibition = await _db.Exhibitions
				.Select(e => new ExhibitionDisplayModel
				{
					Id = e.Id,
					Title = e.Title,
					ExhibitionTimestamp = e.ExhibitionTimestamp,
					Games = e.Games.Select(g => new ExhibitionDisplayModel.GameModel
					{
						Id = g.Id,
						DisplayName = g.DisplayName,
					}).ToList(),
					Contributors = e.Contributors.Select(c => new ExhibitionDisplayModel.UserModel
					{
						Id = c.Id,
						UserName = c.UserName
					}).OrderBy(e => e.UserName).ToList(),
					Urls = e.Urls.Select(u => new ExhibitionDisplayModel.UrlModel
					{
						Url = u.Url,
						Type = u.Type,
						DisplayName = u.DisplayName
					}).ToList(),
				})
				.SingleOrDefaultAsync(e => e.Id == Id);

			if (exhibition is null)
			{
				return NotFound();
			}

			Exhibition = exhibition;

			return Page();
		}
    }
}