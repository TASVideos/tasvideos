using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class SubmissionsModel(ApplicationDbContext db, IWikiPages wikiPages) : BasePageModel
{
	public List<RssSubmission> Submissions { get; set; } = [];
	public async Task<IActionResult> OnGet()
	{
		var filter = TASVideos.Pages.Submissions.IndexModel.SubmissionSearchRequest.Default;
		Submissions = await db.Submissions
			.Where(s => filter.Contains(s.Status))
			.ByMostRecent()
			.Select(s => new RssSubmission(s.Id, s.TopicId, s.CreateTimestamp, s.Title))
			.Take(10)
			.ToListAsync();

		foreach (var sub in Submissions)
		{
			sub.Wiki = (await wikiPages.SubmissionPage(sub.Id))!;
		}

		return Rss();
	}

	public record RssSubmission(int Id, int? TopicId, DateTime CreateTimestamp, string Title)
	{
		public IWikiPage? Wiki { get; set; }
	}
}
