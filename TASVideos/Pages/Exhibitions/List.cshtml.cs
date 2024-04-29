using TASVideos.Pages.Exhibitions.Models;

namespace TASVideos.Pages.Exhibitions;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	public List<ExhibitionDisplayModel> Exhibitions { get; set; } = new();
	public async Task OnGet()
	{
		Exhibitions = await db.Exhibitions
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
			.ToListAsync();
	}
}
