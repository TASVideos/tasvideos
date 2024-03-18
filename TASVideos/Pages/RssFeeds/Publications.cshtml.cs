using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.RssFeeds.Models;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class PublicationsModel(
	ApplicationDbContext db,
	IWikiPages wikiPages) : BasePageModel
{
	public List<RssPublication> Publications { get; set; } = new();
	public async Task<IActionResult> OnGet()
	{
		var minTimestamp = DateTime.UtcNow.AddDays(-60);
		Publications = await db.Publications
			.ByMostRecent()
			.Where(p => p.CreateTimestamp >= minTimestamp)
			.Select(p => new RssPublication
			{
				Id = p.Id,
				MovieFileSize = p.MovieFile.Length,
				CreateTimestamp = p.CreateTimestamp,
				Title = p.Title,
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
					.Select(pr => pr.Value)
					.ToList()
			})
			.ToListAsync();

		foreach (var pub in Publications)
		{
			pub.Wiki = (await wikiPages.PublicationPage(pub.Id))!;
		}

		PageResult pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}
}
