using TASVideos.Data.Entity.Exhibition;
using TASVideos.Pages.Exhibitions.Models;

namespace TASVideos.Pages.Exhibitions.Drafts;

public class PublishModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public ExhibitionDisplayModel Exhibition { get; set; } = new();
	public async Task<IActionResult> OnGet()
	{
		var exhibition = await db.Exhibitions
			.Select(e => new ExhibitionDisplayModel
			{
				Id = e.Id,
				PublishId = e.PublishId,
				Status = e.Status,
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

		if (exhibition.Status == ExhibitionStatus.Published)
		{
			return RedirectToPage("View", new { Id = exhibition.PublishId });
		}

		if (exhibition.Status != ExhibitionStatus.Accepted)
		{
			return RedirectToPage("Drafts/View", new { exhibition.Id });
		}

		Exhibition = exhibition;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var exhibition = await db.Exhibitions.SingleOrDefaultAsync(e => e.Id == Id);

		if (exhibition is null)
		{
			return NotFound();
		}

		if (exhibition.Status == ExhibitionStatus.Published)
		{
			return RedirectToPage("View", new { Id = exhibition.PublishId });
		}

		if (exhibition.Status != ExhibitionStatus.Accepted)
		{
			return RedirectToPage("Drafts/View", new { exhibition.Id });
		}

		exhibition.Status = ExhibitionStatus.Published;
		exhibition.PublishId = await db.Exhibitions.MaxAsync(e => e.PublishId) + 1;
		exhibition.PublicationTimestamp = DateTime.UtcNow;

		var topic = await db.ForumTopics.FirstOrDefaultAsync(t => t.Id == exhibition.TopicId);
		if (topic is not null)
		{
			string topicTitle = $"#{exhibition.Id}: {exhibition.Title}";
			topic.Title = topicTitle;
		}

		await db.SaveChangesAsync();

		return RedirectToPage("View", new { Id = exhibition.PublishId });
	}
}
