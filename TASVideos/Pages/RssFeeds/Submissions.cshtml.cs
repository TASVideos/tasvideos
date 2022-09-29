using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.RssFeeds.Models;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class SubmissionsModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IWikiPages _wikiPages;

	public SubmissionsModel(
		ApplicationDbContext db,
		IWikiPages wikiPages,
		AppSettings settings)
	{
		_db = db;
		_wikiPages = wikiPages;
		BaseUrl = settings.BaseUrl;
	}

	public List<RssSubmission> Submissions { get; set; } = new();
	public string BaseUrl { get; set; }
	public async Task<IActionResult> OnGet()
	{
		var filter = SubmissionSearchRequest.Default;
		Submissions = await _db.Submissions
			.Where(s => filter.Contains(s.Status))
			.ByMostRecent()
			.Select(s => new RssSubmission
			{
				Id = s.Id,
				TopicId = s.TopicId,
				CreateTimestamp = s.CreateTimestamp,
				Title = s.Title,
			})
			.Take(10)
			.ToListAsync();

		foreach (var sub in Submissions)
		{
			sub.Wiki = (await _wikiPages.SubmissionPage(sub.Id))!;
		}

		PageResult pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}
}
