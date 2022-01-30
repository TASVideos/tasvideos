using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.RssFeeds.Models;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class SubmissionsModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public SubmissionsModel(
		ApplicationDbContext db,
		AppSettings settings)
	{
		_db = db;
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
				Wiki = s.WikiContent!
			})
			.Take(10)
			.ToListAsync();

		PageResult pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}
}
