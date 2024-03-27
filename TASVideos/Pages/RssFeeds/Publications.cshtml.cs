using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class PublicationsModel(
	ApplicationDbContext db,
	IWikiPages wikiPages) : BasePageModel
{
	public List<RssPublication> Publications { get; set; } = [];
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
				Files = p.Files
					.Select(pf => new RssPublication.File(pf.Path, pf.Type))
					.ToList(),
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

	public class RssPublication
	{
		public IWikiPage Wiki { get; set; } = null!;

		public int Id { get; init; }
		public DateTime CreateTimestamp { get; init; }
		public string Title { get; init; } = "";

		public List<string> TagNames { get; init; } = [];

		public int MovieFileSize { get; init; }
		public string ScreenshotPath => Files.First(f => f.Type == FileType.Screenshot).Path;

		public double RatingCount => Ratings.Count / 2.0;
		public double RatingMin => Ratings.Any() ? Ratings.Min() : 0;
		public double RatingMax => Ratings.Any() ? Ratings.Max() : 0;
		public double RatingAverage => Ratings.Any() ? Math.Round(Ratings.Average(), 2) : 0;

		public List<string> StreamingUrls { get; init; } = [];

		internal List<File> Files { get; init; } = [];

		internal List<double> Ratings { get; init; } = [];

		internal record File(string Path, FileType Type);
	}
}
