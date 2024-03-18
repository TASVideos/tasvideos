using Microsoft.AspNetCore.Authorization;
using TASVideos.Data;
using TASVideos.Pages.Activity.Model;

namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public IEnumerable<ActivitySummaryModel> Judges { get; set; } = new List<ActivitySummaryModel>();
	public IEnumerable<ActivitySummaryModel> Publishers { get; set; } = new List<ActivitySummaryModel>();

	public async Task OnGet()
	{
		Judges = await db.Submissions
			.Where(s => s.JudgeId.HasValue)
			.GroupBy(s => s.Judge!.UserName)
			.Select(s => new ActivitySummaryModel
			{
				UserName = s.Key,
				Count = s.Count(),
				LastActivity = s.Max(ss => ss.CreateTimestamp)
			})
			.ToListAsync();

		Publishers = await db.Publications
			.GroupBy(p => p.Submission!.Publisher!.UserName)
			.Select(p => new ActivitySummaryModel
			{
				UserName = p.Key,
				Count = p.Count(),
				LastActivity = p.Max(pp => pp.CreateTimestamp)
			})
			.ToListAsync();
	}
}
