using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.RssFeeds.Models;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class PublicationsModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public PublicationsModel(
		ApplicationDbContext db,
		AppSettings settings)
	{
		_db = db;
		BaseUrl = settings.BaseUrl;
	}

	public List<RssPublication> Publications { get; set; } = new();
	public string BaseUrl { get; set; }
	public async Task<IActionResult> OnGet()
	{
		var minTimestamp = DateTime.UtcNow.AddDays(-60);
		Publications = await _db.Publications
			.ByMostRecent()
			.Where(p => p.CreateTimestamp >= minTimestamp)
			.Select(p => new RssPublication
			{
				Id = p.Id,
				MovieFileSize = p.MovieFile.Length,
				CreateTimestamp = p.CreateTimestamp,
				Title = p.Title,
				Wiki = p.WikiContent!,
				TagNames = p.PublicationTags.Select(pt => pt.Tag!.DisplayName).ToList(),
				Files = p.Files.Select(pf => new RssPublication.File
				{
					Path = pf.Path,
					Type = pf.Type
				}).ToList(),
				StreamingUrls = p.PublicationUrls
					.Where(pu => pu.Type == PublicationUrlType.Streaming)
					.Where(pu => pu.Url != null)
					.Select(pu => pu.Url!)
					.ToList(),
				Ratings = p.PublicationRatings
					.Select(pr => new RssPublication.Rating
					{
						Value = pr.Value,
						Type = pr.Type
					})
					.ToList()
			})
			.ToListAsync();

		PageResult pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}
}
