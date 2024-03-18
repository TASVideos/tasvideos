using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Activity.Model;

namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class JudgesModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public string UserName { get; set; } = "";

	public IReadOnlyCollection<SubmissionEntryModel> Submissions { get; set; } = [];

	public async Task OnGet()
	{
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
	}
}
