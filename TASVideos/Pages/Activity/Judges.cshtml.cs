namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class JudgesModel(ApplicationDbContext db) : BasePageModel
{
	public List<SubmissionEntry> Submissions { get; set; } = [];

	[FromRoute]
	public string UserName { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		if (string.IsNullOrWhiteSpace(UserName))
		{
			return NotFound();
		}

		var user = await db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
		if (user is null)
		{
			return NotFound();
		}

		Submissions = await db.Submissions
			.ThatHaveBeenJudgedBy(UserName)
			.Select(s => new SubmissionEntry(
				s.Id,
				s.CreateTimestamp,
				s.Title,
				s.Status))
			.ToListAsync();

		return Page();
	}

	public record SubmissionEntry(int Id, DateTime CreateTimestamp, string Title, SubmissionStatus Status);
}
