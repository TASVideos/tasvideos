using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data;
using TASVideos.Pages.Exhibitions.Drafts.Models;

namespace TASVideos.Pages.Exhibitions.Drafts;

public class ViewModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IWikiPages _wikiPages;

	public ViewModel(ApplicationDbContext db, IWikiPages wikiPages)
	{
		_db = db;
		_wikiPages = wikiPages;
	}

	[FromRoute]
	public int Id { get; set; }
	public ExhibitionDraftDisplayModel Exhibition { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var exhibition = await _db.Exhibitions
			.Where(e => e.Id == Id)
			.Select(e => new ExhibitionDraftDisplayModel
			{
				Id = e.Id,
				Title = e.Title,
				TopicId = e.TopicId,
				ExhibitionTimestamp = e.ExhibitionTimestamp,
				CreateTimestamp = e.CreateTimestamp,
				Games = e.Games.Select(g => new ExhibitionDraftDisplayModel.GameModel
				{
					Id = g.Id,
					DisplayName = g.DisplayName
				}).ToList(),
				Contributors = e.Contributors.Select(c => new ExhibitionDraftDisplayModel.UserModel
				{
					Id = c.Id,
					UserName = c.UserName
				}).ToList(),
				Urls = e.Urls.Select(u => new ExhibitionDraftDisplayModel.UrlModel
				{
					Type = u.Type,
					DisplayName = u.DisplayName,
					Url = u.Url,
				}).ToList(),
				Files = e.Files.Select(f => new ExhibitionDraftDisplayModel.FileModel
				{
					Type = f.Type,
					Description = f.Description,
					Path = f.Path,
				}).ToList(),
			})
			.SingleOrDefaultAsync();

		if (exhibition == null)
		{
			return NotFound();
		}

		Exhibition = exhibition;

		return Page();
	}
}