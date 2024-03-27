using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class SubmissionsModel(ApplicationDbContext db, IWikiPages wikiPages) : BasePageModel
{
	public List<RssSubmission> Submissions { get; set; } = [];
	public async Task<IActionResult> OnGet()
	{
		var filter = SubmissionSearchRequest.Default;
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

		PageResult pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}

	public record RssSubmission(int Id, int? TopicId, DateTime CreateTimestamp, string Title)
	{
		public IWikiPage Wiki { get; set; } = null!;
	}
}
