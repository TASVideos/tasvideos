using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.Pages.Submissions.RejectionReasons;

[AllowAnonymous]
public class ReasonModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public ReasonModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public IEnumerable<SubmissionEntry> Submissions { get; set; } = new List<SubmissionEntry>();
	public string RejectionReason { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var reason = await _db.SubmissionRejectionReasons.SingleOrDefaultAsync(r => r.Id == Id);
		if (reason is null)
		{
			return NotFound();
		}

		RejectionReason = reason.DisplayName;
		Submissions = await _db.Submissions
			.Where(s => s.RejectionReasonId == Id)
			.Select(s => new SubmissionEntry(s.Id, s.Title))
			.ToListAsync();

		return Page();
	}

	public record SubmissionEntry(int SubmissionId, string SubmissionTitle);
}
