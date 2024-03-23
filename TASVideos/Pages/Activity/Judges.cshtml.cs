using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Activity.Model;

namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class JudgesModel(ApplicationDbContext db) : BasePageModel
{
	public IReadOnlyCollection<SubmissionEntryModel> Submissions { get; set; } = [];

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
			.Select(s => new SubmissionEntryModel
			{
				Id = s.Id,
				CreateTimestamp = s.CreateTimestamp,
				Title = s.Title,
				Status = s.Status
			})
			.ToListAsync();

		return Page();
	}
}
