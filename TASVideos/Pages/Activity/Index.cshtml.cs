namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public List<ActivityEntry> Judges { get; set; } = [];
	public List<ActivityEntry> Publishers { get; set; } = [];

	public async Task OnGet()
	{
		Judges = await db.Submissions
			.Where(s => s.JudgeId.HasValue)
			.GroupBy(s => s.Judge!.UserName)
			.Select(s => new ActivityEntry(
				s.Key,
				s.Count(),
				s.Max(ss => ss.CreateTimestamp)))
			.ToListAsync();

		Publishers = await db.Publications
			.GroupBy(p => p.Submission!.Publisher!.UserName)
			.Select(p => new ActivityEntry(
				p.Key,
				p.Count(),
				p.Max(pp => pp.CreateTimestamp)))
			.ToListAsync();
	}

	public record ActivityEntry(string? UserName, int Count, DateTime LastActivity);
}
