using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;

namespace TASVideos.Pages.Submissions.RejectionReasons;

[AllowAnonymous]
public class ReasonModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public IEnumerable<SubmissionEntry> Submissions { get; set; } = new List<SubmissionEntry>();
	public string RejectionReason { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var reason = await db.SubmissionRejectionReasons.SingleOrDefaultAsync(r => r.Id == Id);
		if (reason is null)
		{
			return NotFound();
		}

		RejectionReason = reason.DisplayName;
		Submissions = await db.Submissions
			.Where(s => s.RejectionReasonId == Id)
			.Select(s => new SubmissionEntry(s.Id, s.Title))
			.ToListAsync();

		return Page();
	}

	public record SubmissionEntry(int SubmissionId, string SubmissionTitle);
}
