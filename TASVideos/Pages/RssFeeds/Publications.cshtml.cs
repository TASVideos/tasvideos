using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class PublicationsModel(ApplicationDbContext db, IWikiPages wikiPages) : BasePageModel
{
	public List<RssPublication> Publications { get; set; } = [];
	public async Task<IActionResult> OnGet()
	{
		var minTimestamp = DateTime.UtcNow.AddDays(-60);
		Publications = await db.Publications
			.ByMostRecent()
			.Where(p => p.CreateTimestamp >= minTimestamp)
			.Select(p => new RssPublication(
				p.Id,
				p.MovieFile.Length,
				p.CreateTimestamp,
				p.Title,
				p.PublicationTags.Select(pt => pt.Tag!.DisplayName).ToList(),
				p.Files
					.Select(pf => new PubFile(pf.Path, pf.Type))
					.ToList(),
				p.PublicationUrls
					.Where(pu => pu.Type == PublicationUrlType.Streaming)
					.Where(pu => pu.Url != null)
					.Select(pu => pu.Url!)
					.ToList(),
				p.PublicationRatings
					.Select(pr => pr.Value)
					.ToList()))
			.ToListAsync();

		foreach (var pub in Publications)
		{
			pub.Wiki = (await wikiPages.PublicationPage(pub.Id))!;
		}

		return Rss();
	}

	public record RssPublication(
		int Id, int MovieFileSize, DateTime CreateTimestamp, string Title, List<string> TagNames, List<PubFile> Files, List<string> StreamingUrls, List<double> Ratings)
	{
		public IWikiPage? Wiki { get; set; }
		public string ScreenshotPath => Files.First(f => f.Type == FileType.Screenshot).Path;
		public double RatingMin => Ratings.Any() ? Ratings.Min() : 0;
		public double RatingMax => Ratings.Any() ? Ratings.Max() : 0;
		public double RatingAverage => Ratings.Any() ? Math.Round(Ratings.Average(), 2) : 0;
	}

	public record PubFile(string Path, FileType Type);
}
